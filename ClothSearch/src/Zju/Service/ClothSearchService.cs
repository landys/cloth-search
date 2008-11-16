﻿using System;
using System.Collections.Generic;
using Zju.Util;
using Zju.Dao;
using Zju.Domain;

namespace Zju.Service
{
    public class ClothSearchService : IClothSearchService
    {
        private IClothDao clothDao;

        public ClothSearchService()
        {

        }

        public ClothSearchService(IClothDao clothDao)
        {
            this.clothDao = clothDao;
        }

        #region IClothSearchService Members

        public List<Cloth> SearchByText(string words, ColorEnum colors, ShapeEnum shapes)
        {
            List<List<Cloth>> clothLists = new List<List<Cloth>>();

            if (colors != ColorEnum.NONE)
            {
                List<Cloth> clothesByColor = clothDao.FindAllByColors(colors);
                if (clothesByColor.Count > 0)
                {
                    clothLists.Add(clothesByColor);
                }
                else
                {
                    // empty list
                    return clothesByColor;
                }
            }


            if (shapes != ShapeEnum.NONE)
            {
                List<Cloth> clothesByShape = clothDao.FindAllByShapes(shapes);
                if (clothesByShape.Count > 0)
                {
                    clothLists.Add(clothesByShape);
                }
                else
                {
                    // empty list
                    return clothesByShape;
                }
            }
            

            if (!String.IsNullOrEmpty(words))
            {
                string[] patterns = words.Split(new char[] { ',', ' ', '\t' });
                List<List<Cloth>> clothListsByWords = new List<List<Cloth>>();
                foreach (string pattern in patterns)
                {
                    if (!string.IsNullOrEmpty(pattern))
                    {
                        List<Cloth> clothesByPattern = clothDao.FindAllByPattern(pattern);

                        if (clothesByPattern.Count > 0)
                        {
                            clothListsByWords.Add(clothesByPattern);
                        }
                    }
                }
                if (clothListsByWords.Count > 0)
                {
                    clothLists.Add(unionClothLists(clothListsByWords));
                }
                else
                {
                    // empty list
                    return new List<Cloth>();
                }
            }
            
            return intersectClothLists(clothLists);
        }

        public List<Cloth> SearchByPicColor(int[] colorVector)
        {
            SortedDictionary<int, List<Cloth>> sortClothes = new SortedDictionary<int, List<Cloth>>();
            List<Cloth> allClothes = clothDao.FindAll();
            foreach (Cloth cloth in allClothes)
            {
                int md = calcManhattanDistance(colorVector, cloth.ColorVector);
                if (md <= SearchConstants.ColorMDLimit)
                {
                    if (!sortClothes.ContainsKey(md))
                    {
                        sortClothes[md] = new List<Cloth>();
                    }
                    sortClothes[md].Add(cloth);
                }
            }

            List<Cloth> clothes = new List<Cloth>();
            foreach (List<Cloth> cs in sortClothes.Values)
            {
                clothes.AddRange(cs);
            }

            return clothes;
        }

        public List<Cloth> SearchByPicTexture(float[] textureVector)
        {
            SortedDictionary<float, List<Cloth>> sortClothes = new SortedDictionary<float, List<Cloth>>();
            List<Cloth> allClothes = clothDao.FindAll();
            foreach (Cloth cloth in allClothes)
            {
                float md = calcManhattanDistance(textureVector, cloth.TextureVector);
                if (md <= SearchConstants.TextureMDLimit)
                {
                    if (!sortClothes.ContainsKey(md))
                    {
                        sortClothes[md] = new List<Cloth>();
                    }
                    sortClothes[md].Add(cloth);
                }
            }

            List<Cloth> clothes = new List<Cloth>();
            foreach (List<Cloth> cs in sortClothes.Values)
            {
                clothes.AddRange(cs);
            }

            return clothes;
        }

        #endregion

        private int calcManhattanDistance(int[] v1, int[] v2)
        {
            if (v1 == null || v2 == null || v1.Length != v2.Length)
            {
                return int.MaxValue;
            }

            int md = 0;
            int n = v1.Length;
            for (int i = 0; i < n; ++i)
            {
                md += (v1[i] >= v2[i] ? v1[i] - v2[i] : v2[i] - v1[i]);
            }

            return md;
        }

        private float calcManhattanDistance(float[] v1, float[] v2)
        {
            if (v1 == null || v2 == null || v1.Length != v2.Length)
            {
                return float.MaxValue;
            }

            float md = 0.0f;
            int n = v1.Length;
            for (int i = 0; i < n; ++i)
            {
                md += (v1[i] >= v2[i] ? v1[i] - v2[i] : v2[i] - v1[i]);
            }

            return md;
        }

        /// <summary>
        /// Each cloth list in <code>clothLists</code> should not contain duplicate element, or the algorithm will be acted correctly.
        /// </summary>
        /// <param name="clothLists"></param>
        /// <returns></returns>
        private List<Cloth> intersectClothLists(List<List<Cloth>> clothLists)
        {
            if (clothLists.Count == 0)
            {
                return new List<Cloth>();
            }
            if (clothLists.Count == 1)
            {
                return clothLists[0];
            }

            Dictionary<Cloth, int> tc = new Dictionary<Cloth, int>();
            foreach (List<Cloth> clothList in clothLists)
            {
                foreach (Cloth cloth in clothList)
                {
                    if (!tc.ContainsKey(cloth))
                    {
                        tc[cloth] = 1;
                    }
                    else
                    {
                        tc[cloth]++;
                    }
                }
            }

            int nLists = clothLists.Count;
            List<Cloth> result = new List<Cloth>();
            foreach (KeyValuePair<Cloth, int> kvp in tc)
            {
                if (kvp.Value == nLists)
                {
                    result.Add(kvp.Key);
                }
            }

            return result;
        }

        private List<Cloth> unionClothLists(List<List<Cloth>> clothLists)
        {
            if (clothLists.Count == 0)
            {
                return new List<Cloth>();
            }
            if (clothLists.Count == 1)
            {
                return clothLists[0];
            }

            HashSet<Cloth> hs = new HashSet<Cloth>();
            foreach (List<Cloth> clothList in clothLists)
            {
                foreach (Cloth cloth in clothList)
                {
                    hs.Add(cloth);
                }
            }

            List<Cloth> result = new List<Cloth>();
            foreach (Cloth cloth in hs)
            {
                result.Add(cloth);
            }

            return result;
        }

        public IClothDao ClothDao
        {
            set { this.clothDao = value; }
        }
    }
}