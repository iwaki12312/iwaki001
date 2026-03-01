"""
Effect sound (SFX) generation script using ElevenLabs Text-to-Sound-Effects API.

API key setup:
  1. Copy work/env.yaml.example to work/.env.yaml
  2. Set ELEVENLABS_API_KEY in work/.env.yaml
  (Alternatively, set the ELEVENLABS_API_KEY environment variable)

Usage:
  cd work/sfx
  python makeSFX.py
"""

import asyncio
import os
import sys
import time
from pathlib import Path

import aiohttp
import yaml


def load_api_key() -> str:
    """Load ElevenLabs API key from environment variable or work/.env.yaml."""
    api_key = os.environ.get("ELEVENLABS_API_KEY")
    if api_key:
        return api_key

    env_path = Path(__file__).parent.parent / ".env.yaml"
    if env_path.exists():
        with open(env_path, encoding="utf-8") as f:
            config = yaml.safe_load(f) or {}
        api_key = config.get("ELEVENLABS_API_KEY")
        if api_key:
            return api_key

    print("ERROR: ELEVENLABS_API_KEY not configured.")
    print("  Create work/.env.yaml from work/env.yaml.example and set your API key.")
    sys.exit(1)


# ---------------------------------------------------------------------------
# Job definitions
# Add entries here to generate sound effects.
# ---------------------------------------------------------------------------
jobs = [
    # Example:
    # {
    #     "out": r"out/GameName/pop.mp3",
    #     "text": "Cute cartoon pop sound, bubbly and playful, suitable for a children's game",
    #     "duration_seconds": 1.5,   # 0.5-30 seconds (optional)
    #     "prompt_influence": 0.4,   # 0.0-1.0 (optional, default 0.3)
    # },
]


async def generate_sfx(session: aiohttp.ClientSession, api_key: str, job: dict) -> None:
    """Call ElevenLabs sound-generation API and save the result."""
    url = "https://api.elevenlabs.io/v1/sound-generation"
    headers = {
        "xi-api-key": api_key,
        "Content-Type": "application/json",
    }
    payload: dict = {"text": job["text"]}
    if "duration_seconds" in job:
        payload["duration_seconds"] = job["duration_seconds"]
    if "prompt_influence" in job:
        payload["prompt_influence"] = job["prompt_influence"]

    async with session.post(url, headers=headers, json=payload) as resp:
        if resp.status != 200:
            body = await resp.text()
            raise RuntimeError(f"API error {resp.status}: {body}")
        data = await resp.read()

    out_path = Path(__file__).parent / job["out"]
    out_path.parent.mkdir(parents=True, exist_ok=True)
    out_path.write_bytes(data)
    print(f"  Saved: {out_path}")


async def main() -> None:
    if not jobs:
        print("No jobs defined. Add entries to the 'jobs' list in makeSFX.py")
        return

    api_key = load_api_key()
    print(f"Generating {len(jobs)} sound effect(s)...")

    async with aiohttp.ClientSession() as session:
        for i, job in enumerate(jobs):
            print(f"[{i + 1}/{len(jobs)}] {job['out']}")
            await generate_sfx(session, api_key, job)
            if i < len(jobs) - 1:
                await asyncio.sleep(1)  # Respect rate limits

    print("Done!")


if __name__ == "__main__":
    asyncio.run(main())
