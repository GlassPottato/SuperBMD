﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperBMD.Rigging;
using OpenTK;
using GameFormatReader.Common;
using SuperBMD.Util;
using Assimp;

namespace SuperBMD.BMD
{
    public class JNT1
    {
        public List<Rigging.Bone> FlatSkeleton { get; private set; }
        public Dictionary<string, int> BoneNameIndices { get; private set; }
        public Rigging.Bone SkeletonRoot { get; private set; }

        public JNT1(EndianBinaryReader reader, int offset)
        {
            BoneNameIndices = new Dictionary<string, int>();
            FlatSkeleton = new List<Rigging.Bone>();

            reader.BaseStream.Seek(offset, System.IO.SeekOrigin.Begin);
            reader.SkipInt32();

            int jnt1Size = reader.ReadInt32();
            int jointCount = reader.ReadInt16();
            reader.SkipInt16();
            int jointDataOffset = reader.ReadInt32();
            int internTableOffset = reader.ReadInt32();
            int nameTableOffset = reader.ReadInt32();

            List<string> names = NameTableIO.Load(reader, offset + nameTableOffset);

            int highestRemap = 0;
            List<int> remapTable = new List<int>();
            reader.BaseStream.Seek(offset + internTableOffset, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < jointCount; i++)
            {
                int test = reader.ReadInt16();
                remapTable.Add(test);

                if (test > highestRemap)
                    highestRemap = test;
            }

            List<Rigging.Bone> tempList = new List<Rigging.Bone>();
            reader.BaseStream.Seek(offset + jointDataOffset, System.IO.SeekOrigin.Begin);
            for (int i = 0; i <= highestRemap; i++)
            {
                tempList.Add(new Rigging.Bone(reader, names[i]));
            }

            for (int i = 0; i < jointCount; i++)
            {
                FlatSkeleton.Add(tempList[remapTable[i]]);
            }

            foreach (Rigging.Bone bone in FlatSkeleton)
                BoneNameIndices.Add(bone.Name, FlatSkeleton.IndexOf(bone));

            reader.BaseStream.Seek(offset + jnt1Size, System.IO.SeekOrigin.Begin);
        }

        public void SetInverseBindMatrices(List<Matrix3x4> matrices)
        {
            for (int i = 0; i < FlatSkeleton.Count; i++)
            {
                FlatSkeleton[i].SetInverseBindMatrix(new Matrix4(matrices[i].Row0, matrices[i].Row1, matrices[i].Row2, new Vector4(0, 0, 0, 1)));
            }
        }

        public JNT1(Assimp.Scene scene)
        {
            BoneNameIndices = new Dictionary<string, int>();
            FlatSkeleton = new List<Rigging.Bone>();
            Assimp.Node root = null;

            for (int i = 0; i < scene.RootNode.ChildCount; i++)
            {
                if (scene.RootNode.Children[i].Name.ToLowerInvariant() == "skeleton_root")
                {
                    root = scene.RootNode.Children[i].Children[0];
                    break;
                }
            }

            if (root == null)
                throw new Exception("Skeleton root was not found. Please make sure the root is under a node called \"skeleton_root.\"");

            SkeletonRoot = AssimpNodesToBonesRecursive(root, null, FlatSkeleton);

            foreach (Rigging.Bone bone in FlatSkeleton)
                BoneNameIndices.Add(bone.Name, FlatSkeleton.IndexOf(bone));
        }

        private Rigging.Bone AssimpNodesToBonesRecursive(Assimp.Node node, Rigging.Bone parent, List<Rigging.Bone> boneList)
        {
            Rigging.Bone newBone = new Rigging.Bone(node, parent);
            boneList.Add(newBone);

            for (int i = 0; i < node.ChildCount; i++)
            {
                newBone.Children.Add(AssimpNodesToBonesRecursive(node.Children[i], newBone, boneList));
            }

            return newBone;
        }

        public void Write(EndianBinaryWriter writer)
        {
            long start = writer.BaseStream.Position;

            writer.Write("JNT1".ToCharArray());
            writer.Write(0); // Placeholder for section size
            writer.Write((short)FlatSkeleton.Count);
            writer.Write((short)-1);

            writer.Write(24); // Offset to joint data, always 24
            writer.Write(0); // Placeholder for remap data offset
            writer.Write(0); // Placeholder for name table offset

            List<string> names = new List<string>();
            foreach (Rigging.Bone bone in FlatSkeleton)
            {
                writer.Write(bone.ToBytes());
                names.Add(bone.Name);
            }

            long curOffset = writer.BaseStream.Position;

            writer.Seek((int)(start + 16), System.IO.SeekOrigin.Begin);
            writer.Write((int)(curOffset - start));
            writer.Seek((int)curOffset, System.IO.SeekOrigin.Begin);

            for (int i = 0; i < FlatSkeleton.Count; i++)
                writer.Write((short)i);

            StreamUtility.PadStreamWithString(writer, 8);

            curOffset = writer.BaseStream.Position;

            writer.Seek((int)(start + 20), System.IO.SeekOrigin.Begin);
            writer.Write((int)(curOffset - start));
            writer.Seek((int)curOffset, System.IO.SeekOrigin.Begin);

            NameTableIO.Write(writer, names);

            StreamUtility.PadStreamWithString(writer, 32);

            long end = writer.BaseStream.Position;
            long length = (end - start);

            writer.Seek((int)start + 4, System.IO.SeekOrigin.Begin);
            writer.Write((int)length);
            writer.Seek((int)end, System.IO.SeekOrigin.Begin);
        }
    }
}
