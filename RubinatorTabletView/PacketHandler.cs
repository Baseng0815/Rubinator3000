using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using RubinatorCore;
using RubinatorCore.Communication;

namespace RubinatorTabletView {

    // handles packet processing
    public class PacketHandler {
        private readonly BluetoothPeerTablet bluetoothPeer;

        private void HandlePacket(Packet packet) {

        }

        private void SendMove(byte move) {
            Packet packet = new Packet(0x01, move);
            bluetoothPeer.SendPacket(packet);
        }

        public PacketHandler(LinearLayout cubeViewLayout, BluetoothPeerTablet bluetoothPeer) {
            this.bluetoothPeer = bluetoothPeer;

            cubeViewLayout.FindViewById<Button>(Resource.Id.button_l).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(RubinatorCore.CubeFace.LEFT));
                SendMove(0x02);
            };
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_li).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(RubinatorCore.CubeFace.LEFT, -1));
                SendMove(0x03);
            };
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_r).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(RubinatorCore.CubeFace.RIGHT));
                SendMove(0x0A);
            };
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_ri).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(RubinatorCore.CubeFace.RIGHT, -1));
                SendMove(0x0B);
            };
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_f).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(RubinatorCore.CubeFace.FRONT));
                SendMove(0x06);
            };
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_fi).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(RubinatorCore.CubeFace.FRONT, -1));
                SendMove(0x07);
            };
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_b).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(RubinatorCore.CubeFace.BACK));
                SendMove(0x0C);
            };
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_bi).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(RubinatorCore.CubeFace.BACK, -1));
                SendMove(0x0D);
            };
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_u).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(RubinatorCore.CubeFace.UP));
                SendMove(0x04);
            };
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_ui).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(RubinatorCore.CubeFace.UP, -1));
                SendMove(0x05);
            };
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_d).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(RubinatorCore.CubeFace.DOWN));
                SendMove(0x08);
            };
            cubeViewLayout.FindViewById<Button>(Resource.Id.button_di).Click += (obj, e) => {
                ((MainActivity)MainActivity.context).cube_view.renderer.AddMove(new Move(RubinatorCore.CubeFace.DOWN, -1));
                SendMove(0x09);
            };

            cubeViewLayout.FindViewById<Button>(Resource.Id.control_syncfromserver).Click += (obj, e) => {
                bluetoothPeer.SendPacket(new Packet(0x04));
            };

            cubeViewLayout.FindViewById<Button>(Resource.Id.control_synctoserver).Click += (obj, e) => {
                Packet packet = new Packet(0x03);
                for (CubeFace face = CubeFace.LEFT; face <= CubeFace.BACK; face++) {
                    for (int tile = 0; tile < 9; tile++) {
                        packet.Data.Add(Convert.ToByte(((MainActivity)MainActivity.context).cube_view.renderer.At(face, tile)));
                    }
                }

                bluetoothPeer.SendPacket(packet);
            };

            cubeViewLayout.FindViewById<Button>(Resource.Id.control_solve).Click += (obj, e) => {
                bluetoothPeer.SendPacket(new Packet(0x06, 0x30));
            };

            cubeViewLayout.FindViewById<Button>(Resource.Id.control_shuffle).Click += (obj, e) => {
                bluetoothPeer.SendPacket(new Packet(0x06, 0x31));
            };
        }
    }
}