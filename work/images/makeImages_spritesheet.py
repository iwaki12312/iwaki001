"""
Sprite sheet generation script using OpenAI Image Generation API.
Generates a wide image with two variants of the same character side by side,
then splits it into {name}_normal.png and {name}_reaction.png.

API key setup:
  1. Copy work/env.yaml.example to work/.env.yaml
  2. Set OPENAI_API_KEY in work/.env.yaml
  (Alternatively, set the OPENAI_API_KEY environment variable)

Usage:
  cd work/images
  python makeImages_spritesheet.py
"""

import asyncio
import base64
import os
import sys
from pathlib import Path

import aiohttp
import yaml
from PIL import Image
import io


def load_api_key() -> str:
    """Load OpenAI API key from environment variable or work/.env.yaml."""
    api_key = os.environ.get("OPENAI_API_KEY")
    if api_key:
        return api_key

    env_path = Path(__file__).parent.parent / ".env.yaml"
    if env_path.exists():
        with open(env_path, encoding="utf-8") as f:
            config = yaml.safe_load(f) or {}
        api_key = config.get("OPENAI_API_KEY")
        if api_key:
            return api_key

    print("ERROR: OPENAI_API_KEY not configured.")
    print("  Create work/.env.yaml from work/env.yaml.example and set your API key.")
    sys.exit(1)


# Common prefix for all sprite sheet jobs.
# Ensures two clearly separated frames on a single wide image.
COMMON_PREFIX = (
    "A sprite sheet image containing exactly 2 versions of the same animal character. "
    "Colorful pop 2D icon, thick black outline, transparent background, for a children's game. "
    "Place the 1st character on the LEFT half, the 2nd character on the RIGHT half. "
    "Both characters have exactly the same design, color, and body shape — "
    "only the mouth differs. "
    "Left: mouth closed, normal expression. Right: mouth wide open, calling out. "
    "Both facing forward, cute design for children aged 0-3. "
    "Draw EXACTLY 2 characters, no more, no smaller sub-frames."
)

# ---------------------------------------------------------------------------
# Job definitions
# ---------------------------------------------------------------------------
jobs = [
    # Example:
    # {
    #     "name": "chicken",
    #     "out_dir": r"out/GameName",
    #     "prompt": f"{COMMON_PREFIX} Animal: chicken. Red comb, white body, yellow beak.",
    # },
]


def count_opaque_blocks(img: Image.Image, threshold: int = 10) -> int:
    """Count horizontally separated opaque regions (rough sprite count check)."""
    import numpy as np
    arr = img.split()[-1] if img.mode == "RGBA" else None
    if arr is None:
        return 1
    alpha = list(arr.getdata())
    width, height = img.size
    columns = [
        any(alpha[y * width + x] > threshold for y in range(height))
        for x in range(width)
    ]
    blocks = 0
    in_block = False
    for filled in columns:
        if filled and not in_block:
            blocks += 1
            in_block = True
        elif not filled:
            in_block = False
    return blocks


async def generate_spritesheet(
    session: aiohttp.ClientSession, api_key: str, job: dict, attempt: int = 1
) -> None:
    """Generate a sprite sheet and split it into normal/reaction PNGs."""
    max_attempts = 3
    url = "https://api.openai.com/v1/images/generate"
    headers = {
        "Authorization": f"Bearer {api_key}",
        "Content-Type": "application/json",
    }
    payload = {
        "model": "gpt-image-1",
        "prompt": job["prompt"],
        "quality": job.get("quality", "medium"),
        "size": "1536x1024",
        "output_format": "png",
        "background": "transparent",
    }

    async with session.post(url, headers=headers, json=payload) as resp:
        if resp.status != 200:
            body = await resp.text()
            raise RuntimeError(f"API error {resp.status}: {body}")
        result = await resp.json()

    image_data = base64.b64decode(result["data"][0]["b64_json"])
    img = Image.open(io.BytesIO(image_data)).convert("RGBA")

    # Verify the sheet contains exactly 2 character blocks
    blocks = count_opaque_blocks(img)
    if blocks != 2 and attempt < max_attempts:
        print(f"    Warning: expected 2 blocks, got {blocks}. Retrying ({attempt}/{max_attempts})...")
        await asyncio.sleep(2)
        return await generate_spritesheet(session, api_key, job, attempt + 1)

    out_dir = Path(__file__).parent / job["out_dir"]
    out_dir.mkdir(parents=True, exist_ok=True)
    name = job["name"]

    # Save original sheet
    sheet_path = out_dir / f"{name}_sheet.png"
    sheet_path.write_bytes(image_data)
    print(f"  Saved sheet: {sheet_path}")

    # Split left/right halves
    w, h = img.size
    half = w // 2
    normal = img.crop((0, 0, half, h))
    reaction = img.crop((half, 0, w, h))

    normal_path = out_dir / f"{name}_normal.png"
    reaction_path = out_dir / f"{name}_reaction.png"
    normal.save(normal_path)
    reaction.save(reaction_path)
    print(f"  Saved: {normal_path}")
    print(f"  Saved: {reaction_path}")


async def main() -> None:
    if not jobs:
        print("No jobs defined. Add entries to the 'jobs' list in makeImages_spritesheet.py")
        return

    api_key = load_api_key()
    print(f"Generating {len(jobs)} sprite sheet(s)...")

    async with aiohttp.ClientSession() as session:
        for i, job in enumerate(jobs):
            print(f"[{i + 1}/{len(jobs)}] {job['name']}")
            await generate_spritesheet(session, api_key, job)

    print("Done!")


if __name__ == "__main__":
    asyncio.run(main())
