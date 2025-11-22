using UnityEngine;

public class VisualizeAttribute : PropertyAttribute
{
    public int width;
    public int height;
    public float cubeSize;    // default cube size
    public float spacing;     // spacing between cubes
    public bool autoFit;      // toggle auto-fit

    public VisualizeAttribute(int width = 16, int height = 16, float cubeSize = 28f, float spacing = 2f, bool autoFit = false)
    {
        this.width = width;
        this.height = height;
        this.cubeSize = cubeSize;
        this.spacing = spacing;
        this.autoFit = autoFit;
    }
}
