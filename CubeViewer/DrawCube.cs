using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Rubinator3000 {
    // State change between the internal cube representation and the draw cube
    // will take place through animated moves containing the move, endstate and animated time
    public struct AnimatedMove {
        public Move Move;
        public Cube EndState;

        // in milliseconds
        public float TurnDuration;

        public override string ToString() {
            return Move.ToString();
        }
    }

    public enum CubeDisplayMode { NONE = 0, FLAT = 1, CUBE = 2 };

    public static class DrawCube {
        // colors[CuboidIndex, FaceIndex]
        private static Cube currentState;

        private static Shader cubeShader, flatShader;

        // a cuboid may be rotated only by one face at a time
        // i.e. rotations like F and R are forbidden, but F and B are fine
        private static float[] faceRotations;
        private static Matrix4[] faceRotationMatrices;

        private static Queue<AnimatedMove> moveQueue;

        private static Vector3[] renderColors;

        private static Task task;

        public static TRSTransformation Transformation;
        public static bool AnimateMoves = true;

        public static CubeDisplayMode DisplayMode = CubeDisplayMode.CUBE;

        // set absolute face rotation
        private static void SetFaceRotation(CubeFace face, float amount) {
            faceRotations[(int)face] = amount;
            faceRotationMatrices[(int)face] = RotationMatrixForFace(face);
        }

        /// <summary>
        /// Returns the rotation matrix for a given face
        /// </summary>
        private static Matrix4 RotationMatrixForFace(CubeFace face) {
            switch (face) {
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
        private static void AnimateMovesTask() {
            while (moveQueue.Count > 0) {
                Log.LogStuff("Animated move executing in animated move task");
                AnimatedMove move = moveQueue.Dequeue();

                if (move.TurnDuration > 0) {
                    Stopwatch watch = new Stopwatch();

                    double anglePerMillisecond = 90 / move.TurnDuration;
                    // if move is null, skip animation and directly set state
                    if (move.Move != null) {
                        if (move.Move.IsPrime)
                            anglePerMillisecond *= -1;

                        watch.Start();

                        // rotate until 90 degrees is hit, then reset rotation and copy cube
                        // also, issue a redraw
                        while (Math.Abs(faceRotations[(int)move.Move.Face]) < 90) {
                            SetFaceRotation(move.Move.Face, (float)(watch.ElapsedMilliseconds * anglePerMillisecond));
                            CubeViewer.Window.Invalidate();
                        }
                    }
                }

                currentState = move.EndState;
                CubeViewer.Window.Invalidate();
                if (move.Move != null)
                    faceRotations[(int)move.Move.Face] = 0;
            }
        }

        public static void Init(Vector3[] _renderColors, Cube cube = null) {
            cubeShader = new Shader("Resources/CubeShader");
            flatShader = new Shader("Resources/FlatShader");

            cubeShader.Bind();

            // texture units to sampler
            for (int i = 0; i < 2; i++)
                cubeShader.Upload(string.Format("texture{0}", i.ToString()), i);

            if (cube != null)
                currentState = cube;
            else
                currentState = new Cube();

            renderColors = _renderColors;

            faceRotations = new float[6];
            faceRotationMatrices = new Matrix4[6];

            Transformation = new TRSTransformation();
            Transformation.Scale = new Vector3(2);

            for (int i = 0; i < 6; i++)
                faceRotationMatrices[i] = Matrix4.Identity;

            moveQueue = new Queue<AnimatedMove>();

            Log.LogStuff("Animation Thread Start");
        }

        public static void StopDrawing() {
            task.Wait();

            Log.LogStuff("Animation Thread Stop");
        }

        /// <summary>
        /// Adds the move to the queue
        /// <p>Acts like state set when no move and no duration is given</p>
        /// </summary>
        public static void AddMove(Cube endState, Move move = null, int duration = 0) {
            // deep copy because otherwise, the arrays would refer to the same memory
            moveQueue.Enqueue(new AnimatedMove { Move = move, EndState = (Cube)endState.Clone(), TurnDuration = Settings.MoveAnimatedTime });

            bool makeNewTask = false;
            if (task != null) {
                if (task.Status != TaskStatus.Running) {
                    makeNewTask = true;
                }
            } else makeNewTask = true;

            if (makeNewTask)
                task = Task.Factory.StartNew(() => AnimateMovesTask());


            if (move != null)
                Log.LogStuff($"Animate Move: {move.ToString()}");
        }

        public static void Draw(View view) {
            // draw cube
            if (DisplayMode == CubeDisplayMode.CUBE) {
                cubeShader.Bind();

                // view and projection matrices
                cubeShader.Upload("viewMatrix", view.ViewMatrix);
                cubeShader.Upload("projectionMatrix", view.ProjectionMatrix);

                // each cuboid
                for (int cuboid = 0; cuboid < CuboidTransformations.Transformations.Length; cuboid++) {
                    if (cuboid == 13) continue;
                    var transform = CuboidTransformations.Transformations[cuboid];
                    var cuboidMat = transform.GetMatrix();

                    // apply face rotations by looking up key by value
                    CubeFace? cuboidFace = null;
                    foreach (var mapping in CuboidTransformations.FaceMappings) {
                        if (mapping.Value.Contains(cuboid) && faceRotations[(int)mapping.Key] != 0)
                            cuboidFace = mapping.Key;
                    }

                    // reset render colors
                    for (int i = 0; i < 6; i++)
                        cubeShader.Upload(string.Format("color[{0}]", i.ToString()),
                            new Vector3(.1f));

                    // cube color data (matrix array)
                    var data = currentState.GetData();

                    // each tile
                    for (CubeFace face = 0; (int)face < 6; face++) {
                        // select positions relevant for current tile face
                        foreach (Position pos in CuboidTransformations.CuboidMappings[transform.Position].Where(x => x.Face == face)) {
                            CubeColor color = data[(int)pos.Face][pos.Tile];
                            cubeShader.Upload(string.Format("color[{0}]", ((int)pos.Face).ToString()),
                                renderColors[(int)color]);
                        }

                        var rotMat = cuboidFace != null ? faceRotationMatrices[(int)cuboidFace] : Matrix4.Identity;
                        var model = CubeTransformations.Transformations[(int)face].GetMatrix() * cuboidMat * rotMat;

                        cubeShader.Upload(string.Format("modelMatrix[{0}]", ((int)face).ToString()), model);
                        cubeShader.Upload("cubeModelMatrix", Transformation.GetMatrix());
                    }

                    // access time for a dict is close to O(1), so no significant performance loss
                    ResourceManager.LoadedModels["cubePlane"].BindVao();
                    ResourceManager.LoadedTextures["cubeBlendFrame"].Bind(0);
                    ResourceManager.LoadedTextures["cubeBumpMap"].Bind(1);

                    GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, 6, (int)6);
                }

            // draw flat
            } else if (DisplayMode == CubeDisplayMode.FLAT) {
                var data = currentState.GetData();

                flatShader.Bind();
                for (CubeFace face = 0; (int)face < 6; face++)
                {
                    for (int tile = 0; tile < 9; tile++)
                    {
                        int ind = (int)face * 9 + tile;
                        flatShader.Upload(string.Format("modelMatrix[{0}]", ind), FlatTransformations.Transformations[(int)face, tile].GetMatrix());

                        CubeColor color = data[(int)face][tile];
                        flatShader.Upload(string.Format("color[{0}]", ind), renderColors[(int)color]);
                    }
                }

                ResourceManager.LoadedModels["flatPlane"].BindVao();
                ResourceManager.LoadedTextures["flatBlendFrame"].Bind(0);

                GL.DrawElementsInstanced(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (IntPtr)0, 54);
            }
        }
    }
}
