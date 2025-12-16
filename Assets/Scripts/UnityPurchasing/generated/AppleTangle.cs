using System;
using UnityEngine;

/// <summary>
/// AppleTangle - iOS用のダミークラス（iOS対応時に正式に生成する）
/// </summary>
public class AppleTangle
{
    private static byte[] data = System.Convert.FromBase64String("AAAA");
    
    public static byte[] Data()
    {
        return data;
    }
}
