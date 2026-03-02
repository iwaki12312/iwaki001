"""
Image generation script using OpenAI Image Generation API.

API key setup:
  1. Copy work/env.yaml.example to work/.env.yaml
  2. Set OPENAI_API_KEY in work/.env.yaml
  (Alternatively, set the OPENAI_API_KEY environment variable)

Usage:
  cd work/images
  python makeImages.py
"""

import asyncio
import base64
import os
import sys
from pathlib import Path

import aiohttp
import yaml


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


# ---------------------------------------------------------------------------
# Job definitions
# Add entries here to generate images.
# ---------------------------------------------------------------------------
jobs = [
    # Example:
    # {
    #     "out": r"out/GameName/sprite_name.png",
    #     "prompt": "Colorful pop 2D illustration, thick black outline, transparent background, "
    #               "cute design for children aged 0-3.",
    #     "quality": "medium",     # "low" | "medium" | "high"
    #     "size": "1024x1024",     # "1024x1024" | "1536x1024" | "1024x1536"
    # },
]


async def generate_image(session: aiohttp.ClientSession, api_key: str, job: dict) -> None:
    """Call OpenAI image generation API and save the result."""
    url = "https://api.openai.com/v1/images/generate"
    headers = {
        "Authorization": f"Bearer {api_key}",
        "Content-Type": "application/json",
    }
    payload = {
        "model": "gpt-image-1",
        "prompt": job["prompt"],
        "quality": job.get("quality", "medium"),
        "size": job.get("size", "1024x1024"),
        "output_format": "png",
        "background": "transparent",
    }

    async with session.post(url, headers=headers, json=payload) as resp:
        if resp.status != 200:
            body = await resp.text()
            raise RuntimeError(f"API error {resp.status}: {body}")
        result = await resp.json()

    image_data = base64.b64decode(result["data"][0]["b64_json"])
    out_path = Path(__file__).parent / job["out"]
    out_path.parent.mkdir(parents=True, exist_ok=True)
    out_path.write_bytes(image_data)
    print(f"  Saved: {out_path}")


async def main() -> None:
    if not jobs:
        print("No jobs defined. Add entries to the 'jobs' list in makeImages.py")
        return

    api_key = load_api_key()
    print(f"Generating {len(jobs)} image(s)...")

    async with aiohttp.ClientSession() as session:
        for i, job in enumerate(jobs):
            print(f"[{i + 1}/{len(jobs)}] {job['out']}")
            await generate_image(session, api_key, job)

    print("Done!")


if __name__ == "__main__":
    asyncio.run(main())
