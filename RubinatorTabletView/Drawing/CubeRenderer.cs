using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OpenTK;
using RubinatorCore;

using OpenTK.Graphics.ES31;

namespace RubinatorTabletView {
    // State change between the internal cube representation and the draw cube
    // will take place through animated moves containing the move, endstate and animated time
    public struct AnimatedMove {
        public Move Move;
        public Cube EndState;

        public AnimatedMove(Move move = null, Cube endState = null) {
            this.Move = move;
            this.EndState = endState;
        }

        public override string ToString() {
            return Move.ToString();
        }
    }

    public enum CubeDisplayMode { NONE = 0, FLAT = 1, CUBE = 2 };

    class CubeRenderer {
        private Cube currentState;

        private Shader cubeShader, flatShader;

        // a cuboid may be rotated only by one face at a time
        // i.e. rotations like F and R are forbidden, but F and B are fine
        private float[] faceRotations;
        private Matrix4[] faceRotationMatrices;

        private Queue<AnimatedMove> moveQueue;

        private Vector3[] renderColors;

        private Thread animateMovesThread;
        private bool running = false;

        public TRSTransformation Transformation;
        public bool AnimateMoves = true;

        public CubeDisplayMode DisplayMode = CubeDisplayMode.CUBE;

        // set absolute face rotation
        private void SetFaceRotation(CubeFace face, float amount) {
            faceRotations[(int)face] = amount;
            faceRotationMatrices[(int)face] = RotationMatrixForFace(face);
        }

        /// <summary>
        /// Returns the rotation matrix for a given face
        /// </summary>
        private Matrix4 RotationMatrixForFace(CubeFace face) {
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
        private void AnimateMovesThread() {
            Log.LogMessage("Animated Thread start");

            while (moveQueue.Count > 0 && running) {
                //Log.LogMessage("Animated move executing in animated move task");
                AnimatedMove animMove;

                animMove = moveQueue.Dequeue();

                AnimatedMove? nextMove = null;

                if (Settings.UseMultiTurn) {
                    if (moveQueue.Count > 0) {
                        AnimatedMove nm = moveQueue.ElementAt(0);
                        if (Cube.IsOpponentFace(animMove.Move.Face, nm.Move.Face)) {
                            nextMove = nm;

                            moveQueue.Dequeue();
                        }
                    }
                }

                // do animation if move is given
                if (animMove.Move != null || (nextMove.HasValue ? nextMove.Value.Move != null : false)) {
                    Stopwatch watch = new Stopwatch();

                    float anglePerMillisecond = 90 / (float)Settings.MoveAnimatedTime;

                    watch.Start();

                    // rotate until wanted angle is hit, then reset rotation and update internal state
                    // also, issue a redraw
                    while (Math.Abs(faceRotations[(int)animMove.Move.Face]) < 90 * Math.Abs(animMove.Move.Count)) {
                        SetFaceRotation(animMove.Move.Face, watch.ElapsedMilliseconds * anglePerMillisecond * animMove.Move.Direction);
                        if (nextMove.HasValue)
                            SetFaceRotation(nextMove.Value.Move.Face, watch.ElapsedMilliseconds * anglePerMillisecond * nextMove.Value.Move.Direction);
                    }

                    // set new state and reset rotation
                    currentState.DoMove(animMove.Move);
                    SetFaceRotation(animMove.Move.Face, 0);

                    if (nextMove.HasValue) {
                        currentState.DoMove(nextMove.Value.Move);
                        SetFaceRotation(nextMove.Value.Move.Face, 0);
                    }

                    // skip animation and directly set end state
                }
                else {
                    if (nextMove.HasValue)
                        currentState = nextMove.Value.EndState;
                    else
                        currentState = animMove.EndState;
                }
            }

            running = false;
        }

        public void Init(Vector3[] _renderColors) {
            cubeShader = new Shader("Shaders/CubeShader");
            flatShader = new Shader("Shaders/FlatShader");

            cubeShader.Bind();

            // texture units to sampler
            for (int i = 0; i < 2; i++)
                //TODO: remove error
                cubeShader.UpoadSampler(string.Format("texture{0}", i.ToString()), i);
            var error = GL.GetError();

            currentState = new Cube();

            renderColors = _renderColors;

            faceRotations = new float[6];
            faceRotationMatrices = new Matrix4[6];

            Transformation = new TRSTransformation();
            Transformation.Scale = new Vector3(2);

            for (int i = 0; i < 6; i++)
                faceRotationMatrices[i] = Matrix4.Identity;

            moveQueue = new Queue<AnimatedMove>();
        }

        public void StopDrawing() {
            running = false;
            if (animateMovesThread != null) {

                animateMovesThread.Join();
            }
        }

        /// <summary>
        /// Adds the end state to the queue
        /// </summary>
        /// <param name="endState"></param>
        public void AddState(Cube endState) {
            lock (moveQueue)
                moveQueue.Enqueue(new AnimatedMove(null, (Cube)endState.Clone()));
        }

        /// <summary>
        /// Adds the move to the queue
        /// </summary>
        public void AddMove(Move move) {
            moveQueue.Enqueue(new AnimatedMove(move, null));

            KeepThreadAlive();
        }

        private void KeepThreadAlive() {
            if (!running) {
                running = true;
                animateMovesThread = new Thread(AnimateMovesThread);
                animateMovesThread.Start();
            }
        }

        public void Draw(View view) {

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

                    GL.DrawArraysInstanced(All.Triangles, 0, 6, 6);                    
                }
                // draw flat
            }
            else if (DisplayMode == CubeDisplayMode.FLAT) {
                var data = currentState.GetData();

                flatShader.Bind();
                for (CubeFace face = 0; (int)face < 6; face++) {
                    for (int tile = 0; tile < 9; tile++) {
                        int ind = (int)face * 9 + tile;
                        //flatShader.Upload(string.Format("modelMatrix[{0}]", ind), FlatTransformations.Transformations[(int)face, tile].GetMatrix());

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