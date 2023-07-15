using System.Runtime.InteropServices;

namespace Nickvision.MPVSharp.Internal;

/// <summary>
/// MPV data node
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public partial struct MPVNode {
    [LibraryImport("libmpv.so.2")]
    private static partial void mpv_free_node_contents(ref MPVNode node);

    [FieldOffset(0)]
    private readonly nint _string;
    [FieldOffset(0)]
    private readonly int _flag;
    [FieldOffset(0)]
    private readonly long _int64;
    [FieldOffset(0)]
    private readonly double _double;
    [FieldOffset(0)]
    private readonly nint _nodeList;
    [FieldOffset(0)]
    private readonly nint _byteArray;
    [FieldOffset(8)]
    public MPVFormat Format;

    public MPVNode(string str, MPVFormat format = MPVFormat.String)
    {
        _string = Marshal.StringToCoTaskMemUTF8(str);
        Format = MPVFormat.String;
    }
    
    public MPVNode(int flag)
    {
        _flag = flag;
        Format = MPVFormat.Flag;
    }

    public MPVNode(long int64)
    {
        _int64 = int64;
        Format = MPVFormat.Int64;
    }

    public MPVNode(double dbl)
    {
        _double = dbl;
        Format = MPVFormat.Double;
    }

    public MPVNode(MPVNodeList list, MPVFormat format)
    {
        Marshal.StructureToPtr(list, _nodeList, true);
        Format = format;
    }

    public MPVNode(MPVByteArray ba)
    {
        Marshal.StructureToPtr(ba, _byteArray, true);
        Format = MPVFormat.ByteArray;
    }

    public static MPVNode FromIntPtr(nint data) => Marshal.PtrToStructure<MPVNode>(data);

    public static void FreeNodeContents(MPVNode node) => mpv_free_node_contents(ref node);
    
    public static explicit operator string?(MPVNode n) => Marshal.PtrToStringUTF8(n._string);

    public static explicit operator bool?(MPVNode n) => n.Format == MPVFormat.Flag ? n._flag == 1 : null;

    public static explicit operator long?(MPVNode n) => n.Format == MPVFormat.Int64 ? n._int64 : null;

    public static explicit operator double?(MPVNode n) => n.Format == MPVFormat.Double ? n._double : null;

    public static explicit operator MPVNode[]?(MPVNode n) => n.Format == MPVFormat.NodeArray ? (MPVNode[]?)Marshal.PtrToStructure<MPVNodeList>(n._nodeList) : null;

    public static explicit operator MPVNodeList?(MPVNode n) => n.Format == MPVFormat.NodeArray || n.Format == MPVFormat.NodeMap ? Marshal.PtrToStructure<MPVNodeList>(n._nodeList) : null;

    public static explicit operator MPVByteArray?(MPVNode n) => n.Format == MPVFormat.ByteArray ? Marshal.PtrToStructure<MPVByteArray>(n._byteArray) : null;

    public override string ToString()
    {
        return Format switch
        {
            MPVFormat.Flag => _flag == 1 ? "True" : "False",
            MPVFormat.Int64 => _int64.ToString(),
            MPVFormat.Double => _double.ToString(),
            MPVFormat.NodeArray => Marshal.PtrToStructure<MPVNodeList>(_nodeList).ToString(),
            MPVFormat.NodeMap => Marshal.PtrToStructure<MPVNodeList>(_nodeList).ToString(),
            MPVFormat.ByteArray => _byteArray.ToString(),
            _ => Marshal.PtrToStringUTF8(_string)
        } ?? "";
    }
}