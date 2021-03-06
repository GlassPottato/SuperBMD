﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperBMD.Materials.Enums;
using SuperBMD.Util;
using OpenTK;

namespace SuperBMD.Materials
{
    public class Material
    {
        public string Name;
        public byte Flag;
        public byte ColorChannelControlsCount;
        public byte NumTexGensCount;
        public byte NumTevStagesCount;
        public IndirectTexturing IndTexEntry;
        public CullMode CullMode;
        public Color[] MaterialColors;
        public ChannelControl[] ChannelControls;
        public Color[] AmbientColors;
        public Color[] LightingColors;
        public TexCoordGen[] TexCoord1Gens;
        public TexCoordGen[] PostTexMatrixGens;
        public TexMatrix[] TexMatrix1;
        public TexMatrix[] TexMatrix2;
        public int[] TextureIndices;
        public TevOrder[] TevOrders;
        public KonstColorSel[] ColorSels;
        public KonstAlphaSel[] AlphaSels;
        public Color[] TevColors;
        public Color[] KonstColors;
        public TevStage[] TevStages;
        public TevSwapMode[] SwapModes;
        public TevSwapModeTable[] SwapTables;
        public Fog FogInfo;
        public AlphaCompare AlphCompare;
        public BlendMode BMode;
        public ZMode ZMode;
        public bool ZCompLoc;
        public bool Dither;
        public NBTScale NBTScale;

        public Material()
        {
            MaterialColors = new Color[2] { new Color(1, 1, 1, 1), new Color(1, 1, 1, 1) };
            ColorChannelControlsCount = 4;
            ChannelControls = new ChannelControl[4];
            for (int i = 0; i < 4; i++)
                ChannelControls[i] = new ChannelControl(false, ColorSrc.Register, LightId.None, DiffuseFn.None, J3DAttenuationFn.None_0, ColorSrc.Register);

            AmbientColors = new Color[2] { new Color(0, 0, 0, 1), new Color(0, 0, 0, 0) };
            LightingColors = new Color[8];
            TexCoord1Gens = new TexCoordGen[8];
            PostTexMatrixGens = new TexCoordGen[8];
            TexMatrix1 = new TexMatrix[10];
            TexMatrix2 = new TexMatrix[20];
            TextureIndices = new int[8] { -1, -1, -1, -1, -1, -1, -1, -1 };
            KonstColors = new Color[4];
            ColorSels = new KonstColorSel[16];
            AlphaSels = new KonstAlphaSel[16];
            TevOrders = new TevOrder[16];
            TevOrders[0] = new TevOrder(TexCoordId.TexCoord0, TexMapId.TexMap0, J3DColorChannelId.Color0);
            TevColors = new Color[16];
            TevStages = new TevStage[16];
            SwapModes = new TevSwapMode[16];
            SwapModes[0] = new TevSwapMode(0, 1);
            SwapTables = new TevSwapModeTable[16];
            SwapTables[0] = new TevSwapModeTable(0, 1, 2, 3);
            SwapTables[1] = new TevSwapModeTable(0, 0, 0, 3);
        }

        public void AddTexGen(TexGenType genType, TexGenSrc genSrc, Enums.TexMatrix mtrx)
        {
            TexCoordGen newGen = new TexCoordGen(genType, genSrc, mtrx);

            for (int i = 0; i < 8; i++)
            {
                if (TexCoord1Gens[i] == null)
                {
                    TexCoord1Gens[i] = newGen;
                    break;
                }

                if (i == 7)
                    throw new Exception($"TexGen array for material \"{ Name }\" is full!");
            }

            NumTexGensCount++;
        }

        public void AddTexMatrix(TexGenType projection, byte type, Vector3 effectTranslation, Vector2 scale, float rotation, Vector2 translation, Matrix4 matrix)
        {
            for (int i = 0; i < 10; i++)
            {
                if (TexMatrix1[i] == null)
                {
                    TexMatrix1[i] = new TexMatrix(projection, type, effectTranslation, scale, rotation, translation, matrix);
                    break;
                }

                if (i == 9)
                    throw new Exception($"TexMatrix1 array for material \"{ Name }\" is full!");
            }
        }

        public void AddTexIndex(int index)
        {
            for (int i = 0; i < 8; i++)
            {
                if (TextureIndices[i] == -1)
                {
                    TextureIndices[i] = index;
                    break;
                }

                if (i == 7)
                    throw new Exception($"TextureIndex array for material \"{ Name }\" is full!");
            }
        }

        public void AddTevStage(TevStageParameters parameters)
        {
            for (int i = 0; i < 16; i++)
            {
                if (TevStages[i] == null)
                {
                    TevStages[i] = new TevStage(parameters);
                    break;
                }

                if (i == 15)
                    throw new Exception($"TevStage array for material \"{ Name }\" is full!");
            }

            NumTevStagesCount++;
        }

        public void Debug_Print()
        {
            Console.WriteLine($"TEV stage count: { NumTevStagesCount }\n\n");

            for (int i = 0; i < 16; i++)
            {
                if (TevStages[i] == null)
                    continue;

                Console.WriteLine($"Stage { i }:");
                Console.WriteLine(TevStages[i].ToString());
            }
        }
    }
}
