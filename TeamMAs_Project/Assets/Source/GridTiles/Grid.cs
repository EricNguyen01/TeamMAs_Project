using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamMAsTD
{
    public class Grid
    {
        private int width;
        private int height;
        private int[,] grid2DArray;

        public Grid(int width, int height)
        {
            this.width = width;
            this.height = height;

            grid2DArray = new int[width, height];
        }

        public int GetWidth()
        {
            return width;   
        }

        public int GetHeight()
        {
            return height;
        }
    }
}
