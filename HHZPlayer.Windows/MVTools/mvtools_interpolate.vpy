# mvtools_interpolate.vpy
# VapourSynth template for MVTools-based frame interpolation.
# This script is intentionally defensive: if MVTools is not available it
# returns the original clip unchanged. Fill parameters and ensure the
# environment has VapourSynth + mvtools installed.

import vapoursynth as vs
core = vs.core

# mpv passes the input clip as the global name `clip` when using
# vapoursynth=file=... in mpv's vf. If not present, we simply raise.
try:
    clip
except NameError:
    raise RuntimeError("No input clip provided. This script is intended to be used with mpv's vapoursynth vf.")

# Parameters (tweak as needed)
pel = 2          # motion search accuracy
sharp = 1        # sharpen for Super
tru = False      # use refine

def safe_mv_interpolate(clip, num=2, den=1):
    try:
        # Try to use classic mvtools interface
        super = core.mv.Super(clip, pel=pel, sharp=sharp)
        vectors = core.mv.Analyse(super, blksize=16, overlap=8, search=3)
        # Flow-based interpolation if available (mvtools2 / flowfps)
        try:
            interpolated = core.mv.FlowFPS(clip, super, vectors, num=num, den=den)
            return interpolated
        except Exception:
            # Fall back to simple frame doubling with MDegrain/MComp if FlowFPS not available
            try:
                interpolated = core.mv.BlockFPS(clip, super, vectors, num=num, den=den)
                return interpolated
            except Exception:
                # As last resort return original clip
                return clip
    except Exception:
        # mvtools not present or failed ¡ª return original clip unmodified
        return clip

# Example: create 2x interpolation (num=2, den=1)
# If you want to change interpolation factor, adjust num/den accordingly.
out = safe_mv_interpolate(clip, num=2, den=1)

# Expose the output clip variable for mpv
clip = out

# End of script
