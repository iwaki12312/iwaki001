// WARNING: Do not modify! Generated file.

using UnityEngine.Purchasing.Security;

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("hOzjYi9DtE5q+MjAXZGeCJhsPJ03DU86CE7L85yLPuMb1Klt7KTJpQLvypVfQUvBE0r5L2LiZMcqyel5BmGPtMi1TJGxOvEjMMBzNV2e7DhXseZ/JoG55xJZ2BBN4G65cvrMHaCD5RI8XXTGrGZFJkPIfoyLl9Pg/XNtdrNi6F59H+GY36N05QND6uTWz3L/D8mKrYS+vOPejmUV+JZMPXs3PqIIz7CrUoDD5cX4W+TIls5ZLE09jPeFs0prpVaVsR/O+tolI3Vb6WpJW2ZtYkHtI+2cZmpqam5raOlqZGtb6WphaelqamuwYUs6Q+0mi6F0f0ZJjD06SSeLbTXPKOyNhwNCf2i0xe5gIiAK9jNKiKYmYiXyWnSVTs8TbuChyGloamtq");
        private static int[] order = new int[] { 11,12,7,13,4,13,9,13,13,9,11,12,12,13,14 };
        private static int key = 107;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
