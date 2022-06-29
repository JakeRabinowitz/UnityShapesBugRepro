using Shapes;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class RadarChart : ImmediateModeShapeDrawer
{
    [Header("Drawing Settings")]
    [Range(0.1f, 10f)] public float radius = 2f;
    public float rotation = 0f;


    [Header("Border")]
    [Range(0.1f, 10f)] public float borderThickness = 1.2f;
    public Color borderColor = Color.black;

    public Color backgroundColor = new Color(0.96f, 0.96f, 0.96f);

    [Header("Fill")]
    public Color fillColor = new Color(0f, 0.8f, 0f, 0.4f);
    public float fillBorderThickness = 0.1f;
    public Color fillBorderColor = Color.black;

    [Header("Guide Lines")]
    [Range(0, 10)] public int numGuideLines = 1;

    [Range(0.1f, 10f)] public float guideLineThickness = 0.3f;
    public Color guideLineColor = new Color(0.7f, 0.7f, 0.7f);

    [Header("Spokes")]
    public bool drawSpokes = true;
    public Color spokeColor = new Color(0.6f, 0.6f, 0.6f);
    [Range(0.1f, 10f)] public float spokeThickness = 0.1f;

    [Header("Contents")]
    public float minValue = -0.1f;
    public float maxValue = 100f;
    public float[] values = new float[]
    {
        42f, 36f, 16f, 35f, 50f, 35f,
    };

    [Header("Axes")]
    public Sprite[] axisIcons = null;
    private SpriteRenderer[] renderers = null;

    private void Start()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        ResetIcons();
    }

    public void ResetIcons()
    {
        if (renderers != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = i < values.Length;
            }
        }
    }

    public float[] Values
    {
        get => values;
        set
        {
            values = value;
            if (renderers != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].enabled = i < values.Length;
                }
            }
        }
    }

    public override void DrawShapes(Camera cam)
    {
        int sides = Mathf.Max(3, Values.Length);
        // Draw.Command enqueues a set of draw commands to render in the given camera
        using (Draw.Command(cam))
        { // all immediate mode drawing should happen within these using-statements
            Draw.ResetAllDrawStates(); // this makes sure no static draw states "leak" over to this scene
            Draw.Matrix = transform.localToWorldMatrix; // this makes it draw in the local space of this transform

            // Draw Border
            Draw.SizeSpace = ThicknessSpace.Noots;
            Draw.ThicknessSpace = ThicknessSpace.Noots;

            // Draw background.
            Draw.RegularPolygon(Vector3.zero, sides, radius, rotation, 0, backgroundColor);

            // Draw guide lines
            /*
             * If num guidelines =
             * 1: draw at 1/2 the radius
             * 2: draw at 1/3 & 2/3 the radius
             * 3: draw at 1/4, 2/4, & 3/4 the radius
             */
            for (int i = 0; i < numGuideLines; i++)
            {
                float proportion = ((float)i + 1) / (numGuideLines + 1);
                Draw.RegularPolygonBorder(Vector3.zero, sides, proportion * radius, guideLineThickness, rotation, 0, guideLineColor);
            }

            // Draw spokes
            if (drawSpokes)
            {
                for (int i = 0; i < sides; i++)
                {
                    float radians = Mathf.PI * 2 * ((float)i / sides) + rotation;
                    Vector3 end = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0) * radius;
                    Draw.Line(Vector3.zero, end, spokeThickness, spokeColor);

                    if (renderers != null && i < renderers.Length)
                    {
                        renderers[i].transform.localPosition = end + (end.normalized * 0.3f);
                    }
                }
            }

            // Draw border
            Draw.Thickness = borderThickness;
            Draw.RegularPolygonBorder(Vector3.zero, sides, radius, borderThickness, rotation, 0, borderColor);

            // Draw values.
            using (PolygonPath path = new PolygonPath())
            {
                using (PolylinePath linePath = new PolylinePath())
                {
                    for (int i = 0; i < Values.Length; i++)
                    {
                        float proportion = Mathf.InverseLerp(minValue, maxValue, Values[i]);
                        float distance = proportion * radius;
                        float radians = ((float)i / Values.Length).RotationsToRadians() + rotation;

                        Vector2 point = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * distance;

                        path.AddPoint(point);
                        linePath.AddPoint(point);
                    }

                    Draw.Polygon(path, fillColor);
                    Draw.Polyline(linePath, true, fillBorderThickness, fillBorderColor);

                }
            }
        }
    }
}

public static class FloatExtensions
{
    public static float AngleToRadians(this float angle)
    {
        return angle * Mathf.Deg2Rad;
    }

    public static float RadiansToAngle(this float radians)
    {
        return radians * Mathf.Rad2Deg;
    }

    public static float RotationsToRadians(this float rotations)
    {
        return rotations * Mathf.PI * 2;
    }
}