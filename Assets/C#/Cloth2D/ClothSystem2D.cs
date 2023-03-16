using System.Collections.Generic;
using UnityEngine;

namespace Assets.C_.Cloth2D
{
    public class ClothSystem2D
    {
        public bool HoldPoint; // if the mouse is held down with an active point
        public PointMass2D activePoint; // the point being currently held.

        public Vector2 MousePos;
        // spring itteration count
        public int springItteration;

        public List<PointMass2D> points = new List<PointMass2D>();

        #region Properties
        public List<PointMass2D> Points
        {
            get { return points; }
        }
        #endregion

        public ClothSystem2D(int itteration)
        {
            springItteration = itteration; // number of constraint itterations
        }

        // update the physics
        public void Update(float dt, float iterations, Vector2 mousePos)
        {
            // If we havn't regenerated a new cloth yet
            if (points == null)
            {
                return;
            }
            // solve the spring constraints multiple times
            // higher iteration counts give better accuracy at the cost of processing time
            for (int x = 0; x < springItteration; x++)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    points[i].SolveConstraints();
                }
            }

            // update for each pointmass's position
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i] != activePoint) // don't update the selected point
                {
                    points[i].Update(dt);
                }
                else // make the selected point follow the mouse
                {
                    // update selected point position to follow the mouse
                    Vector2 dist = points[i].Position - mousePos;
                    points[i].Position = points[i].Position - dist;
                }                  
            }
        }

        public void Draw()
        {
            foreach (PointMass2D point in points)
            {
                point.Draw();
            }
        }

        #region mouse interaction
        public bool SelectClosestMousePoint(Vector2 mouse, float mouseRadius)
        {
            //activePoint;
            float closest = Mathf.Infinity;
            float dist = 0;
            foreach (PointMass2D point in points)
            {
                dist = PointMouseDistance(point.Position, mouse);
                if (mouseRadius > dist && dist < closest)
                {
                    activePoint = point;
                    closest = dist;
                }
            }

            if (activePoint != null)
            {
                HoldPoint = true;
            }
            else
            {
                HoldPoint = false;
            }

            return HoldPoint;
        }

        public void UnselectMousePoint()
        {
            if (HoldPoint)
            {
                activePoint = null;
                HoldPoint = false;
            }
        }

        private float PointMouseDistance (Vector2 point, Vector2 mouse)
        {
            return Vector2.Distance(point, mouse);
        }
        #endregion

        public void AddPoint(PointMass2D p)
        {
            points.Add(p);
        }

        public PointMass2D AddPoint(float x, float y, float mass)
        {
            PointMass2D point = new PointMass2D(x,y,mass);
            points.Add(point);
            return point;
        }

        public void RemovePoint(PointMass2D p)
        {
            points.Remove(p);
        }
        
        public void UpdateSpringCoefficient(float spring_k)
        {
            foreach (PointMass2D point in points)
            {
                point.SetSpringCoefficient(spring_k);
            }
        }

        public void UpdateSpringColour(int colourType)
        {
            foreach (PointMass2D point in points)
            {
                point.SetSpringColouring(colourType);
            }
        }

        public void UpdateMousePos(Vector2 pos)
        {
            MousePos = pos;
        }

        public void ToggleStaticOnSelected()
        {
            // toggle static status
            activePoint.SetFixed(!activePoint.IsStatic);
        }

        public void ClearCloth()
        {
            // clear all springs
            foreach (PointMass2D point in points)
            {
                point.RemovePoint();
            }
            points.Clear();
            points = null;
        }
    }
}
