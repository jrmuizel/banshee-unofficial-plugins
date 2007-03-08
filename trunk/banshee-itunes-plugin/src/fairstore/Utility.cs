#region LICENSE
// Copyright (C) 2003 Ecmel Ercan
// Copyright (C) 2005 Jon Lech Johansen
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Ecmel Ercan (ecmel@ercansoy.com)
// Jon Lech Johansen (jon@nanocrew.net)
#endregion

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Net;
using System.Security.Cryptography.X509Certificates;

class TrustAllCertificatesPolicy : System.Net.ICertificatePolicy
{
    public TrustAllCertificatesPolicy()
    {

    }

    public bool CheckValidationResult( ServicePoint sp, X509Certificate cert,
                                       WebRequest req, int problem )
    {
        return true;
    }
}

public class Utility
{
    public static string StringFromResource (
        System.Reflection.Assembly assembly,
        string resource_name )
    {
        if (assembly == null)
            assembly = System.Reflection.Assembly.GetCallingAssembly ();

        System.IO.Stream s = assembly.GetManifestResourceStream (resource_name);

        if (s == null)
            throw new ArgumentException ("Cannot get resource file '" +
                                         resource_name +
                                         "'",
                                         "resource_name");

        System.IO.StreamReader sr = new System.IO.StreamReader (s);

        int size = (int) s.Length;
        char[] buffer = new char[size];
        sr.Read (buffer, 0, size);
        sr.Close ();
        s.Close ();

        return new string (buffer);
    }

    public static bool CreateDirectory (string path)
    {
        bool success = true;
        if (!System.IO.Directory.Exists (path))
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo (path);
            try
            {
                di.Create ();
            }
            catch (Exception ex)
            {
                success = false;
            }
        }
        return success;
    }

    public static string MarkupSanitize( string text )
    {
        string [,] reps = new string[,]
        {
            { "&", "&amp;" },
            { "<", "&lt;" },
            { ">", "&gt;" },
            { "\"", "&quot;" },
        };

        for( int i = 0; i < reps.Length / 2; i++ )
            text = Regex.Replace( text, reps[ i, 0 ], reps[ i, 1 ] );

        return text;
    }

    public static void LeReverse( byte [] arr, int index, int length )
    {
        if( BitConverter.IsLittleEndian )
        {
            Array.Reverse( arr, index, length );
        }
    }

    public static void LeReverse( byte [] arr )
    {
        LeReverse( arr, 0, arr.Length );
    }

    public static int IndexOf( byte [] buf, uint val )
    {
        for( int i = 0; i < (buf.Length - 3); i += 4 )
        {
            if( BitConverter.ToUInt32( buf, i ) == val )
                return i;
        }

        return -1;
    }

    public static void RijndaelDecrypt( byte [] Buf, int Offset, int Count,
                                        byte [] Key, byte [] IV )
    {
        Rijndael alg = Rijndael.Create();
        alg.Mode = CipherMode.CBC;
        alg.Padding = PaddingMode.None;

        MemoryStream ms = new MemoryStream();

        ICryptoTransform ct = alg.CreateDecryptor( Key, IV );
        CryptoStream cs = new CryptoStream( ms, ct, CryptoStreamMode.Write );
        cs.Write( Buf, Offset, (Count / 16) * 16 );
        cs.Close();

        ms.ToArray().CopyTo( Buf, Offset );
    }
}
