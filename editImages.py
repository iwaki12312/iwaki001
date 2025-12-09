"""
editImages.py - AI Image Generation Prompts for EggHatch Game

This file contains prompts for generating sprites/images used in the EggHatch mini-game.
Use these prompts with AI image generation tools (like DALL-E, Midjourney, Stable Diffusion, etc.)
to create consistent game assets.
"""

# ===================================================================
# Baby Animal Sprite Generation Prompts
# ===================================================================

BABY_ANIMAL_PROMPT = """
Generate a cute baby animal sprite for a children's game.

Requirements:
- Style: Cartoon, colorful, child-friendly (target audience: ages 1-2)
- Format: PNG with transparent background
- Size: 512x512 pixels minimum
- Animal should be facing forward
- Large, expressive eyes
- Cheerful, happy expression
- Simple, clean design suitable for toddlers
- Bright, vibrant colors

IMPORTANT: The newborn baby animal should NOT have any eggshells attached to it.
The animal should appear clean and complete, without any shell fragments or broken eggshell pieces on its body.

Examples of animals to generate:
- Baby chick (yellow, fluffy)
- Baby duckling (yellow with orange beak)
- Baby penguin (black and white with cute flippers)
- Baby turtle (green with patterned shell)
- Baby dinosaur (colorful, friendly-looking)
- Baby dragon (fantasy, cute style)
- Baby rabbit (fluffy, long ears)
- Baby elephant (gray with big ears and small trunk)
"""

# ===================================================================
# Egg Sprite Generation Prompts
# ===================================================================

EGG_SPRITE_PROMPT = """
Generate an egg sprite for a children's game.

Requirements:
- Style: Cartoon, simple, clean
- Format: PNG with transparent background
- Size: 512x512 pixels minimum
- Egg should be centered
- Smooth, rounded shape
- Can have patterns or colors (spots, stripes, solid colors)
- Should look intact and whole

Note: This is the egg BEFORE hatching. It should be complete and uncracked.
"""

# ===================================================================
# Rare Baby Animal Sprite Generation Prompts
# ===================================================================

RARE_BABY_ANIMAL_PROMPT = """
Generate a RARE/SPECIAL baby animal sprite for a children's game.

Requirements:
- Style: Cartoon, colorful, child-friendly but more special/unique
- Format: PNG with transparent background
- Size: 512x512 pixels minimum
- Should look more exotic or special than normal animals
- Can include fantasy elements (sparkles, unique colors, etc.)
- Large, expressive eyes
- Cheerful, happy expression
- Bright, vibrant, possibly iridescent or special colors

IMPORTANT: The newborn baby animal should NOT have any eggshells attached to it.
The rare animal should appear pristine and complete, without any shell fragments or broken eggshell pieces on its body.

Examples of rare animals to generate:
- Golden baby phoenix
- Rainbow-colored baby unicorn
- Sparkly baby dragon
- Crystal-winged baby butterfly creature
- Shimmering baby mermaid creature
- Glowing baby star creature
"""

# ===================================================================
# Crack/Break Sprite Generation Prompts
# ===================================================================

CRACK_SPRITE_PROMPT = """
Generate egg crack overlay sprites for animation (3 stages).

Requirements:
- Style: Cartoon, simple cracks
- Format: PNG with transparent background
- Size: 512x512 pixels minimum
- Create 3 progressive crack stages:
  1. Small crack (single line)
  2. Medium cracks (multiple lines, more spread)
  3. Large cracks (egg about to break, many cracks)
- Cracks should be dark gray or black lines
- Background must be transparent so it overlays on egg sprite
"""

# ===================================================================
# Usage Instructions
# ===================================================================

"""
HOW TO USE THESE PROMPTS:

1. Copy the relevant prompt above
2. Paste it into your AI image generation tool
3. Specify the animal type you want (e.g., "baby chick", "baby dragon")
4. Generate the image
5. Download as PNG
6. Import into Unity project at Assets/Games/11_EggHatch/Sprites/
7. Configure import settings:
   - Texture Type: Sprite (2D and UI)
   - Pixels Per Unit: 100
   - Compression: None
   - Filter Mode: Bilinear

IMPORTANT REMINDERS:
- Baby animals should NEVER have eggshell fragments attached
- All sprites should have transparent backgrounds
- Maintain consistent art style across all sprites
- Test in-game to ensure sprites look good at various sizes
"""
