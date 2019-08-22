using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Rubinator3000
{
    // State change between the internal cube representation and the draw cube
    // will take place through animated moves containing the move, endstate and animated time
    public struct AnimatedMove
    {
        public Move Move;
        public Cube EndState;

        // in milliseconds
        public float TurnDuration;
    }

    public static class DrawCube
    {
        // plane stuff
        private static PlaneTransformations planeTransformations;

        // colors[CuboidIndex, FaceIndex]
        private static Cube currentState;

        // a cuboid may be rotated only by one face at a time
        // i.e. rotations like F and R are forbidden, but F and B are fine
        private static float[] faceRotations;
        private static Matrix4[] faceRotationMatrices;

        private static Queue<AnimatedMove> moveQueue;

        private static Vector3[] renderColors;

        public static TRSTransformation Transformation;
        public static bool AnimateMoves = true;

        // set absolute face rotation
        private static void SetFaceRotation(CubeFace face, float amount)
        {
            faceRotations[(int)face] = amount;
            faceRotationMatrices[(int)face] = RotationMatrixForFace(face);
        }

        /// <summary>
        /// Returns the rotation matrix for a given face
        /// </summary>
        private static Matrix4 RotationMatrixForFace(CubeFace face)
        {
            switch (face)
            {
                case CubeFace.LEFT:
                    return Matrix4.CreateRotationX(Utility.ToRad(faceRotations[(int)face]));
                case CubeFace.RIGHT:
                    return Matrix4.CreateRotationX(Utility.ToRad(-faceRotations[(int)face]));
                case CubeFace.UP:
                    return Matrix4.CreateRotationY(Utility.ToRad(-faceRotations[(int)face]));
                case CubeFace.DOWN:
                    return Matrix4.CreateRotationY(Utility.ToRad(faceRotations[(int)face]));
                case CubeFace.FRONT:
                    return Matrix4.CreateRotationZ(Utility.ToRad(-faceRotations[(int)face]));
                case CubeFace.BACK:
                    return Matrix4.CreateRotationZ(Utility.ToRad(faceRotations[(int)face]));

                default:
                    return Matrix4.Identity;
            }
        }

        // do animated moves
        private static void AnimatedMoveThread()
        {
            while (true)
            {
                if (moveQueue.Count > 0)
                {
                    AnimatedMove move = moveQueue.Dequeue();

                    if (move.TurnDuration > 0)
                    {
                        Stopwatch watch = new Stopwatch();

                        double anglePerMillisecond = 90 / move.TurnDuration;
                        if (move.Move.IsPrime)
                            anglePerMillisecond *= -1;

                        watch.Start();

                        // rotate until 90 degrees is hit, then reset rotation and copy cube
                        while (Math.Abs(faceRotations[(int)move.Move.Face]) < 90)
                        {
                            SetFaceRotation(move.Move.Face, (float)(watch.ElapsedMilliseconds * anglePerMillisecond));
                        }
                    }

                    SetState(move.EndState);
                    faceRotations[(int)move.Move.Face] = 0;
                }
            }
        }

        /// <summary>
        /// Load resources, set render colors and default state
        /// </summary>
        public static void Init(Vector3[] _renderColors, Cube cube = null)
        {
            planeTransformations = new PlaneTransformations();

            currentState = new Cube();

            if (cube != null)
                SetState(cube);

            renderColors = _renderColors;

            faceRotations = new float[6];
            faceRotationMatrices = new Matrix4[6];

            Transformation = new TRSTransformation();
            Transformation.Scale = new Vector3(2);

            for (int i = 0; i < 6; i++)
                faceRotationMatrices[i] = Matrix4.Identity;

            moveQueue = new Queue<AnimatedMove>();

            Thread animatedMoveThread = new Thread(new ThreadStart(AnimatedMoveThread));
            animatedMoveThread.Start();
        }

        public static void SetState(Cube cube)
        {
            // deep copy because otherwise, the arrays would refer to the same memory
            currentState = Utility.DeepClone<Cube>(cube);
        }

        /// <summary>
        /// Adds the animated move to the queue
        /// </summary>
        public static void AddAnimatedMove(AnimatedMove move)
        {
            moveQueue.Enqueue(move);
        }

        public static void Draw(Shader shader)
        {
            // each cuboid
            for (int cuboid = 0; cuboid < CuboidTransformations.Transformations.Length; cuboid++) {
                if (cuboid == 13) continue;
                var transform = CuboidTransformations.Transformations[cuboid];
                var cuboidMat = transform.GetMatrix();

                // apply face rotations by looking up key by value
                CubeFace? cuboidFace = null;
                foreach (var mapping in CuboidTransformations.FaceMappings)
                {
                    if (mapping.Value.Contains(cuboid) && faceRotations[(int)mapping.Key] != 0)
                        cuboidFace = mapping.Key;
                }

                // reset render colors
                for (int i = 0; i < 6; i++)
                    shader.Upload("color[" + i.ToString() + "]",
                        new Vector3(.1f));

                // each tile
                for (CubeFace face = 0; face < CubeFace.NUMBER_FACES; face++)
                {
                    // select positions relevant for current tile face
                    foreach (Position pos in CuboidTransformations.CuboidMappings[transform.Position].Where(x => x.Face == face))
                    {
                        CubeColor color = currentState.GetData()[(int)pos.Face][pos.Tile];
                        shader.Upload("color[" + ((int)pos.Face).ToString() + "]",
                            renderColors[(int)color]);
                    }

                    var rotMat = cuboidFace != null ? faceRotationMatrices[(int)cuboidFace] : Matrix4.Identity;
                    var model = planeTransformations[(int)face].GetMatrix() * cuboidMat * rotMat;

                    shader.Upload("modelMatrix[" + ((int)face).ToString() + "]", model);
                    shader.Upload("cubeModelMatrix[" + ((int)face).ToString() + "]", Transformation.GetMatrix());
                }

                // access time for a dict is close to O(1), so no significant performance loss
                ResourceManager.LoadedModels["cubePlane"].BindVao();
                ResourceManager.LoadedTextures["cubeBlendFrame"].Bind(0);
                ResourceManager.LoadedTextures["cubeBumpMap"].Bind(1);

                GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, (int)CubeFace.NUMBER_FACES);
            }
        }

    }
}
