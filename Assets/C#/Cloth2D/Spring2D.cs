using UnityEngine;

namespace Assets.C_.Cloth2D
{
    public class Spring2D
    {
        public PointMass2D p1;
        public PointMass2D p2;

        public float RestingDistance;
        public float SpringCoefficient;
        public float TearSensitivity;

        // variables for visualising the spring in unity
        private GameObject obj;
        private LineRenderer springRenderer;
        private (SpringColourType, Color) SpringColour;
        // boolean variable to draw it or not

        public Spring2D(PointMass2D a, PointMass2D b, float springCoefficient, float tearSensitivity, Material defaultMaterial, (SpringColourType, Color) springColour)
        {// points are passed as references
            p1 = a;
            p2 = b;

            RestingDistance = (b.Position - a.Position).magnitude;
            SpringCoefficient = springCoefficient;
            TearSensitivity = tearSensitivity;

            // for rendering
            SpringColour = springColour;

            obj = new GameObject();
            springRenderer = obj.AddComponent<LineRenderer>();
            springRenderer.material = defaultMaterial;
            springRenderer.SetWidth(0.1f, 0.1f);
        }

        // update line positions
        public void UpdateLine()
        {
            springRenderer.SetPosition(0, p1.Position);
            springRenderer.SetPosition(1, p2.Position);
        }

        public void UpdateSpring()
        {
            // calculate the distance between the points
            float diffX = p1.X - p2.X;
            float diffY = p1.Y - p2.Y;
            float dist = Mathf.Sqrt(diffX * diffX + diffY * diffY);

            if (dist > RestingDistance) // only apply if the cloth is getting extended, cloth doesn't have compression resistance in the same way it has to getting pulled. There's some but this feels like a fair simplification
            {
                // if the distance is more than the tear sensitivity the cloth tears
                if (dist > TearSensitivity)
                {
                    p1.RemoveLink(this);
                    DisableLineRenderer();
                }

                // Find the spring discplacment using the length of the spring and subtracting the resting length
                float displacment = (RestingDistance - dist) / dist;

                // the mass for all points is the same so this is just an optimization
                float mass = p1.Mass;
                // the effect of the spring depends on the mass of the attached object
                float k = (SpringCoefficient / mass);

                // apply the effect of the spring for each point in opposite directions (to pull them together)
                if (!p1.IsStatic)
                {
                    p1.X += k * (diffX * displacment);
                    p1.Y += k * (diffY * displacment);
                }
                if (!p2.IsStatic)
                {
                    p2.X -= k * (diffX * displacment);
                    p2.Y -= k * (diffY * displacment);
                }
            }
        }       

        public void DisableLineRenderer()
        {
            springRenderer = null;
            obj.SetActive(false);
        }

        public void UpdateLineColour(int type)
        {
            // toggle between the default colour otherwise make the spring transparent so you can see the current colour type of spring
            switch (type)
            {
                case (int)SpringColourType.All:
                    {
                        springRenderer.SetColors(Color.white, Color.white);
                        springRenderer.SetWidth(0.1f, 0.1f);
                        break;
                    }
                case (int)SpringColourType.Structural:
                    {
                        if (type == (int)SpringColour.Item1)
                        {
                            springRenderer.SetColors(SpringColour.Item2, SpringColour.Item2);
                            springRenderer.SetWidth(0.15f, 0.15f);
                        }
                        else
                        {
                            springRenderer.SetColors(Color.clear, Color.clear);
                            springRenderer.SetWidth(0.1f, 0.1f);
                        }
                        break;
                    }
                case (int)SpringColourType.Shear:
                    {
                        if (type == (int)SpringColour.Item1)
                        {
                            springRenderer.SetColors(SpringColour.Item2, SpringColour.Item2);
                            springRenderer.SetWidth(0.15f, 0.15f);
                        }
                        else
                        {
                            springRenderer.SetColors(Color.clear, Color.clear);
                            springRenderer.SetWidth(0.1f, 0.1f);
                        }
                        break;
                    }
                case (int)SpringColourType.Flexion:
                    {
                        if (type == (int)SpringColour.Item1)
                        {
                            springRenderer.SetColors(SpringColour.Item2, SpringColour.Item2);
                            springRenderer.SetWidth(0.15f, 0.15f);
                        }
                        else
                        {
                            springRenderer.SetColors(Color.clear, Color.clear);
                            springRenderer.SetWidth(0.1f, 0.1f);
                        }
                        break;
                    }
                default:
                    Debug.LogError("Colourtype value is outside of expected value: " + type);
                    break;
            }
        }

        public void RemoveSpring()
        {
            springRenderer.enabled = false;
            springRenderer = null;
            GameObject.Destroy(obj);
        }
    }
}
