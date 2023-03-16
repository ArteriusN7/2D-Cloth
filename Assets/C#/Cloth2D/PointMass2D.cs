using System.Collections.Generic;
using UnityEngine;

namespace Assets.C_.Cloth2D
{
    public class PointMass2D
    {
        private Vector2 accXY; // force accumulator variable
        private Vector2 position;
        private Vector2 previousPosition;

        private float mass = 1;
        private float inverseMass;

        // all the connected springs to this point
        private List<Spring2D> springs = new List<Spring2D>();

        private bool isStatic = false;
        private Vector2 pinPos;

        #region Properties

        public Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;
            }
        }
        public float X
        {
            get { return position.x; }
            set { position.x = value; }
        }
        public float Y
        {
            get { return position.y; }
            set { position.y = value; }
        }
        public float Mass
        {
            get { return mass; }
            set
            {
                mass = value;
                inverseMass = 1 / mass;
            }
        }
        public float InverseMass
        {
            get { return inverseMass; }
        }
        public float Velocity
        {
            get { return (position - previousPosition).magnitude; }
        }
        public bool IsStatic
        {
            get { return isStatic; }
        }
        public int SpringCount
        {
            get { return springs.Count; }
        }

        #endregion

        public PointMass2D(Vector2 pos)
        {
            position = pos;
            previousPosition = pos;
            accXY = Vector2.zero;

            Mass = 1;
        }

        public PointMass2D(float x, float y, float _mass)
        {
            position = new Vector2(x,y);
            previousPosition = position;
            accXY = Vector2.zero;

            Mass = _mass;
        }

        public PointMass2D(float x, float y)
        {
            position = new Vector2(x, y);
            previousPosition = position;
            accXY = Vector2.zero;

            Mass = 1;
        }

        // The update function is used to update the physics of the PointMass.
        // motion is applied, and links are drawn here
        public void Update(float dt)
        {
            if (isStatic)
            {
                return;
            }

            this.ApplyForce(0, mass * ClothController2D.GRAVITY);

            Vector2 velXY = position - previousPosition;

            // dampen velocity
            velXY *= 0.99f;

            // deltatime squared
            float dtSqrd = dt * dt;

            // calculate the next position using verlet intigration
            Vector2 nextXY = position + velXY + accXY * dtSqrd;

            // reset variables
            previousPosition = position;
            position = nextXY;
            accXY = Vector2.zero;
        }

        public void Draw()
        {
            foreach (Spring2D spring in springs)
            {
                spring.UpdateLine();
            }
        }

        public void SolveConstraints()
        {
            // Since multiple springs are connected to all points we run the spring updates multiple times to increase the stability and accuracy of the system
            for (int i = 0; i < springs.Count; i++)
            {
                Spring2D p = springs[i];
                p.UpdateSpring();
            }
        }

        public void AttachTo(PointMass2D p, float spring_k, float tearSens, Material lineMat, (SpringColourType, Color) springColour) // send colour and type as a tuple
        {
            Spring2D spring = new Spring2D(this, p, spring_k, tearSens, lineMat, springColour);
            springs.Add(spring);
        }

        public void RemoveLink(Spring2D spring)
        {
            springs.Remove(spring);
        }

        public void ApplyForce(float fX, float fY)
        {
            // g = m/s^2
            // m = kg
            // f = N
            // acceleration = force / mass
            // Could do a optimization with (force * inverse mass) but more concerned with getting the physics right and can leave that as future work
            accXY.x += fX / mass;
            accXY.y += fY / mass;
        }

        public void PinTo(Vector2 pos)
        {
            isStatic = true;
            pinPos = pos;
        }

        public void SetFixed(bool fix)
        {
            isStatic = fix;
        }

        public void SetSpringCoefficient(float spring_k)
        {
            foreach (Spring2D spring in springs)
            {
                spring.SpringCoefficient = spring_k;
            }
        }

        public void SetSpringColouring(int colourType)
        {
            foreach (Spring2D spring in springs)
            {
                spring.UpdateLineColour(colourType);
            }
        }

        // used to clear data to generate a new cloth
        public void RemovePoint()
        {
            foreach (Spring2D spring in springs)
            {
                spring.RemoveSpring();
            }
        }
    }
}
