using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.C_.Cloth2D
{
    public enum SpringColourType
    {
        All = 0,
        Structural = 1,
        Shear = 2,
        Flexion = 3
    }

    public class ClothController2D : MonoBehaviour
    {
        public Text springTypeTextUI;
        public Material defaultSpringMaterial;
        public Color structuralSpringColour;
        public Color shearSpringColour;
        public Color flexionSpringColour;
        private int currentColourType = 0;

        public List<PointMass2D> points = new List<PointMass2D>();

        [SerializeField] private Vector2 mousePos2D;

        // mouse radious for interacting with points
        public float mouseInfluenceRadius = 1;

        public static float GRAVITY = -9.8f;

        // The size of the cloth is mesured in the number of points in either x or y axis
        public int height = 10;
        public int width = 15;
        float xStep = 1f; // vertical distance between points when generated
        float yStep = 1f; // Horizontal distance between points when generated

        // reflects the mass value to the UI
        [SerializeField] private Text currentPointMass;
        // the mass value for each point
        public float pointMass = 1f;

        [SerializeField, Range(0.005f, 1f)]
        private float spring_k = 0.03f;
        public float clothTearResistance = 6; // How far apart points ave to be before they rip apart / the spring breaks
        [SerializeField] private int constraintAccuracy = 6;

        // boolean toggles
        public bool staticTopRow = true;
        public bool staticTopRight = false;
        public bool staticTopCorners = false;

        private ClothSystem2D sim;

        #region Properties
        public float SpringCoefficent
        {
            get { return spring_k; }
            set
            {
                spring_k = value;
                UpdateSpringCoefficient(spring_k);
            }
        }
        public Vector2 MousePos2D
        {
            get { return mousePos2D; }
            set
            {
                mousePos2D = value;
                UpdateMousePos(mousePos2D);
            }
        }
        public int ConstraintAccuracy
        {
            get { return constraintAccuracy; }
            set
            {
                constraintAccuracy = value;
                sim.springItteration = constraintAccuracy;
            }
        }

        public Vector2 MousePos
        {
            get { return mousePos2D; }
        }
        #endregion

        #region GUI interface

        public void GUIUpdateSpringCoefficient(System.Single i_spring_k)
        {
            SpringCoefficent = i_spring_k;
        }

        public void GUIXStep(System.Single xstep)
        {
            xStep = xstep;
        }

        public void GUIYStep(System.Single ystep)
        {
            yStep = ystep;
        }

        public void GUIPointMass(System.Single mass)
        {
            pointMass = mass;
            if (currentPointMass != null)
            {
                currentPointMass.text = pointMass.ToString();
            }
        }

        public void GUIPointMassString(string mass)
        {
            pointMass = float.Parse(mass);
            if (currentPointMass != null)
            {
                currentPointMass.text = pointMass.ToString();
            }
        }

        public void GUIConstraintItterations(System.Single iterations)
        {
            ConstraintAccuracy = (int)(iterations + 0.5f);
        }

        public void GUIClothTearResistance(System.Single resistance)
        {
            clothTearResistance = resistance;
        }

        public void GUIClothWidth(System.Single _width)
        {
            width = (int)(_width + 0.5f);
        }

        public void GUIClothHeight(System.Single _height)
        {
            height = (int)(_height + 0.5f);
        }

        public void GUIStaticTopRowToggle(Boolean value)
        {
            staticTopRow = value;
        }

        public void GUIStaticTopCornersToggle(Boolean value)
        {
            staticTopCorners = value;
        }

        public void GUIStaticTopRightToggle(Boolean value)
        {
            staticTopRight = value;
        }

        public void GUIRegenerateCloth()
        {
            GenerateNewCloth();
        }

        #endregion

        #region monobehaviour

        public void Awake()
        {
            Initialize();
        }

        public void Update()
        {
            // update mouse position
            Vector3 pos = Input.mousePosition;
            pos.z = Mathf.Abs(Camera.main.transform.position.z); //Camera.main.nearClipPlane+11; // if the mouse is to close it doesn't get detected by the camera transformation, +11 to get it in line with the cloth position roughly. Seems a good value for the adjustment is the magnitude of the cameras z-position
            mousePos2D = Camera.main.ScreenToWorldPoint(pos);

            #region left mousebutton input
            if (Input.GetMouseButtonDown(0))
            {
                sim.SelectClosestMousePoint(mousePos2D, mouseInfluenceRadius);
            }
            if (Input.GetMouseButtonUp(0))
            {
                sim.UnselectMousePoint();
            }
            #endregion

            // If you're holding a point you can make it static with a right click
            if (Input.GetMouseButton(0) && sim.HoldPoint && Input.GetMouseButtonDown(1))
            {
                sim.ToggleStaticOnSelected();
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                GenerateNewCloth();
            }

            // toggle spring rendering colour
            if (Input.GetKeyDown(KeyCode.T))
            {               
                sim.UpdateSpringColour(currentColourType);
                if (springTypeTextUI != null)
                {
                    string name = Enum.GetName(typeof(SpringColourType), currentColourType);
                    springTypeTextUI.text = name;
                    switch (currentColourType)
                    {
                        case 0:
                            springTypeTextUI.color = Color.white;
                            break;
                        case 1:
                            springTypeTextUI.color = structuralSpringColour;
                            break;
                        case 2:
                            springTypeTextUI.color = shearSpringColour;
                            break;
                        case 3:
                            springTypeTextUI.color = flexionSpringColour;
                            break;
                    }
                }
                currentColourType++;
                if (currentColourType > (int)SpringColourType.Flexion)
                {
                    currentColourType = 0;
                }
            }

        }

        public void FixedUpdate()
        { // runs the physics system
            RunSystem();
        }

        private void OnValidate()
        {
            // only validate while the simulation is running
            if (sim != null)
            {   // runs the setter property when a value changes in the inspector
                SpringCoefficent = spring_k;
                MousePos2D = mousePos2D;
                ConstraintAccuracy = constraintAccuracy;
            }       
        }

        #endregion

        public void Initialize()
        {
            structuralSpringColour = Color.green;
            shearSpringColour = new Color(0 ,180 ,255); // light blue
            flexionSpringColour = Color.yellow;

            sim = new ClothSystem2D(constraintAccuracy);

            // create the cloth
            GenerateClothGrid();
        }

        public void RunSystem()
        {
            sim.Update(Time.fixedDeltaTime, 3, mousePos2D);
            sim.Draw();       
        }

        public void GenerateClothGrid()
        {
            // initial cloth height/y-position
            float y = 0f;
            // create the grid, having a 2d array for generation makes it easy to connect the points with springs
            PointMass2D[,] pointGrid = new PointMass2D[height, width];
            for (int j = 0; j < height; j++)
            {
                float x = 0f; // initial x-position
                for (int i = 0; i < width; i++)
                {
                    pointGrid[j, i] = sim.AddPoint(x, y, pointMass);

                    // Check for points to set static
                    if ((staticTopRow && j == 0) || (staticTopRight && j == 0 && i == (width - 1)) || (staticTopCorners && j == 0 && (i == 0 || i == width-1)))
                    {
                        pointGrid[j, i].SetFixed(true);
                    }
                    x = x + xStep;                  
                }
                y -= yStep;
            }

            // Each mass is connected to it's neighbours with a spring
            // Using the [i, j] notation from (Provot 1995). 
            for (int j = 0; j < height - 1; j++)
            {
                for (int i = 0; i < width - 1; i++)
                {
                    PointMass2D p = pointGrid[j, i];
                    PointMass2D pRight = pointGrid[j, i + 1];
                    PointMass2D pDown = pointGrid[j + 1, i];
                    PointMass2D pDownRight = pointGrid[j + 1, i + 1];

                    p.AttachTo(pRight, spring_k, clothTearResistance, defaultSpringMaterial, (SpringColourType.Structural , structuralSpringColour)); // structural spring
                    p.AttachTo(pDown, spring_k, clothTearResistance, defaultSpringMaterial, (SpringColourType.Structural, structuralSpringColour)); // structural spring
                    p.AttachTo(pDownRight, spring_k, clothTearResistance, defaultSpringMaterial, (SpringColourType.Shear, shearSpringColour)); // shear spring \
                    pRight.AttachTo(pDown, spring_k, clothTearResistance, defaultSpringMaterial, (SpringColourType.Shear, shearSpringColour)); // shear spring /
                }
            }

            // the right side and bottom side still arn't connected with springs
            for (int j = 0; j < height - 1; j++)
            {
                PointMass2D p1 = pointGrid[j, width - 1];
                PointMass2D p2 = pointGrid[j + 1, width - 1];

                p1.AttachTo(p2, spring_k, clothTearResistance, defaultSpringMaterial, (SpringColourType.Structural, structuralSpringColour)); // structural spring
            }
            for (int i = 0; i < width - 1; i++)
            {
                PointMass2D p1 = pointGrid[height - 1, i];
                PointMass2D p2 = pointGrid[height - 1, i + 1];

                p1.AttachTo(p2, spring_k, clothTearResistance, defaultSpringMaterial, (SpringColourType.Structural, structuralSpringColour)); // structural spring
            }

            // lastly add flexion springs
            for (int j = 0; j < height - 2; j++)
            {
                for (int i = 0; i < width - 2; i++)
                {
                    PointMass2D p = pointGrid[j, i];
                    PointMass2D pFlexRight = pointGrid[j, i + 2];
                    PointMass2D pFlexDown = pointGrid[j + 2, i];

                    p.AttachTo(pFlexRight, spring_k, clothTearResistance, defaultSpringMaterial, (SpringColourType.Flexion, flexionSpringColour)); // flexion spring
                    p.AttachTo(pFlexDown, spring_k, clothTearResistance, defaultSpringMaterial, (SpringColourType.Flexion, flexionSpringColour)); // flexion spring
                }
            }
            // and the flexion springs for the right hand side and bottom
            for (int j = 0; j < height - 2; j++)
            {
                PointMass2D p1 = pointGrid[j, width - 1];
                PointMass2D p2 = pointGrid[j + 2, width - 1];

                p1.AttachTo(p2, spring_k, clothTearResistance, defaultSpringMaterial, (SpringColourType.Flexion, flexionSpringColour)); // structural spring

                PointMass2D p3 = pointGrid[j, width - 2];
                PointMass2D p4 = pointGrid[j + 2, width - 2];

                p3.AttachTo(p4, spring_k, clothTearResistance, defaultSpringMaterial, (SpringColourType.Flexion, flexionSpringColour)); // structural spring
            }
            for (int i = 0; i < width - 2; i++)
            {
                PointMass2D p1 = pointGrid[height - 1, i];
                PointMass2D p2 = pointGrid[height - 1, i + 2];

                p1.AttachTo(p2, spring_k, clothTearResistance, defaultSpringMaterial, (SpringColourType.Flexion, flexionSpringColour)); // structural spring

                PointMass2D p3 = pointGrid[height - 2, i];
                PointMass2D p4 = pointGrid[height - 2, i + 2];

                p3.AttachTo(p4, spring_k, clothTearResistance, defaultSpringMaterial, (SpringColourType.Flexion, flexionSpringColour)); // structural spring
            }
        }

        public void Draw()
        {
            foreach (PointMass2D point in points)
            {
                point.Draw();
            }
        }

        public void UpdateSpringCoefficient(float i_spring_k)
        {
            sim.UpdateSpringCoefficient(i_spring_k);
        }

        public void UpdateSpringColour(int springType)
        {
            sim.UpdateSpringColour(currentColourType);
        }

        public void UpdateMousePos(Vector2 pos)
        {
            sim.UpdateMousePos(pos);
        }

        public void GenerateNewCloth()
        {
            sim.ClearCloth();
            sim = new ClothSystem2D(constraintAccuracy);
            GenerateClothGrid();
        }

        public void OnDrawGizmos()
        {
            if (sim != null)
            {
                if (sim.Points != null)
                {
                    // Visaulize each vertex with a small black sphere
                    //Gizmos.color = Color.black;
                    //Gizmos.color = Color.blue;
                    foreach (PointMass2D point in sim.Points)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawSphere(point.Position, 0.1f);
                        if (point.IsStatic)
                        {
                            Gizmos.color = Color.red;
                            Gizmos.DrawSphere(point.Position, 0.2f);
                        }
                    }

                    // draw a gizmo for the mouse selection
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(mousePos2D, 0.2f);
                }
            }

        }

    }
}
