/*****************************************************************************
 * MP4.cs: MP4 0.1
 *****************************************************************************
 * Copyright (C) 2005 Jon Lech Johansen <jon@nanocrew.net>
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111, USA.
 *****************************************************************************/

using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections;

struct STSZ
{
    public byte [] Dummy;
    public int [] Table;
}

struct STCO
{
    public byte [] Dummy;
    public int [] Table;
}

struct STSC
{
    public byte [] Dummy;
    public int [] FirstChunk;
    public int [] SamplesPerChunk;
    public int [] SampleDescriptionIndex;
}

public class MP4Types
{
    private static uint ToUInt32( string Type )
    {
        byte [] tmp = Encoding.ASCII.GetBytes( Type );
        return BitConverter.ToUInt32( tmp, 0 );
    }

    public static uint UUID = ToUInt32( "uuid" );
    public static uint MOOV = ToUInt32( "moov" );
    public static uint TRAK = ToUInt32( "trak" );
    public static uint UDTA = ToUInt32( "udta" );
    public static uint EDTS = ToUInt32( "edts" );
    public static uint MDIA = ToUInt32( "mdia" );
    public static uint MINF = ToUInt32( "minf" );
    public static uint DINF = ToUInt32( "dinf" );
    public static uint STBL = ToUInt32( "stbl" );
    public static uint SINF = ToUInt32( "sinf" );
    public static uint SCHI = ToUInt32( "schi" );
    public static uint ILST = ToUInt32( "ilst" );

    public static uint DRMS = ToUInt32( "drms" );
    public static uint USER = ToUInt32( "user" );
    public static uint KEY  = ToUInt32( "key " );
    public static uint IVIV = ToUInt32( "iviv" );
    public static uint NAME = ToUInt32( "name" );
    public static uint PRIV = ToUInt32( "priv" );
    public static uint STSZ = ToUInt32( "stsz" );
    public static uint STCO = ToUInt32( "stco" );
    public static uint STSC = ToUInt32( "stsc" );
    public static uint MDAT = ToUInt32( "mdat" );
    public static uint MP4A = ToUInt32( "mp4a" );

    public static uint FREE = ToUInt32( "free" );
    public static uint AKID = ToUInt32( "akID" );
    public static uint GEID = ToUInt32( "geID" );
    public static uint PLID = ToUInt32( "plID" );
    public static uint ATID = ToUInt32( "atID" );
    public static uint CNID = ToUInt32( "cnID" );
    public static uint APID = ToUInt32( "apID" );

    public static bool IsContainer( uint Type )
    {
        if( Type == MOOV || Type == TRAK || Type == UDTA || Type == EDTS ||
            Type == MDIA || Type == MINF || Type == STBL || Type == SINF ||
            Type == SCHI || Type == ILST )
        {
            return true;
        }

        return false;
    }
}

public class InvalidBoxSizeException : Exception
{
    private MP4Box box;

    public MP4Box Box
    {
        get
        {
            return this.box;
        }
    }

    public InvalidBoxSizeException( MP4Box box ) :
        base( String.Format( "Box {0} has invalid size {1}",
                             box.TypeStr, box.Size ) )
    {
        this.box = box;
    }
}

public class MP4Box
{
    private ulong pos;

    private uint shortsize;
    private uint type;
    private ulong size;
    private byte [] uuid;

    private ArrayList children;
    private object data;

    public ulong Pos
    {
        get
        {
            return this.pos;
        }
    }

    public ulong Size
    {
        get
        {
            return this.size;
        }

        set
        {
            this.size = value;
            if( this.shortsize != 1 )
                this.shortsize = Convert.ToUInt32( this.size );
        }
    }

    public uint Type
    {
        get
        {
            return this.type;
        }

        set
        {
            this.type = value;
        }
    }

    public string TypeStr
    {
        get
        {
            byte [] tmp = BitConverter.GetBytes( this.type );
            return Encoding.ASCII.GetString( tmp );
        }
    }

    public object Data
    {
        get
        {
            return this.data;
        }
    }

    public MP4Box this [uint index]
    {
        get
        {
            if( this.children == null )
                return null;

            for( int i = 0; i < this.children.Count; i++ )
            {
                MP4Box box = (MP4Box)this.children[ i ];
                if( box.Type == index )
                    return box;
                box = box[ index ];
                if( box != null )
                    return box;
            }

            return null;
        }
    }

    public MP4Box this [uint index1, ulong index2]
    {
        get
        {
            if( this.children == null )
                return null;

            for( int i = 0; i < this.children.Count; i++ )
            {
                MP4Box box = (MP4Box)this.children[ i ];
                if( (box.Type == index1) && (index2 >= box.Pos) &&
                    (index2 < (box.Pos + box.Size)) )
                    return box;
                box = box[ index1, index2 ];
                if( box != null )
                    return box;
            }

            return null;
        }
    }

    public MP4Box( BinaryReader br )
    {
        Parse( br );
    }

    private void Parse( BinaryReader br )
    {
        byte [] tmp;

        this.pos = Convert.ToUInt64( br.BaseStream.Position );

        tmp = br.ReadBytes( 4 ); Utility.LeReverse( tmp );
        this.shortsize = BitConverter.ToUInt32( tmp, 0 );

        tmp = br.ReadBytes( 4 );
        this.type = BitConverter.ToUInt32( tmp, 0 );

        if( this.shortsize == 1 )
        {
            tmp = br.ReadBytes( 8 ); Utility.LeReverse( tmp );
            this.size = BitConverter.ToUInt64( tmp, 0 );
        }
        else
        {
            this.size = this.shortsize;
        }

        if( this.size > (Convert.ToUInt64( br.BaseStream.Length ) - this.pos) )
            throw new InvalidBoxSizeException( this );

        if( this.type == MP4Types.UUID )
        {
            this.uuid = br.ReadBytes( 16 );
        }

        if( MP4Types.IsContainer( this.type ) )
        {
            ParseChildren( br );
        }
        else
        {
            try
            {
                this.GetType().InvokeMember( "Parse" + this.TypeStr.ToUpper(),
                    BindingFlags.Default | BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.Instance |
                    BindingFlags.InvokeMethod, null, this,
                    new object[] { br } );
            }
            catch( MissingMethodException )
            {
                int len = Convert.ToInt32( this.size -
                    (Convert.ToUInt64( br.BaseStream.Position ) - this.pos) );
                this.data = br.ReadBytes( len );
            }
        }
    }

    private void ParseChildren( BinaryReader br )
    {
        Stream s = br.BaseStream;
        this.children = new ArrayList();

        while( Convert.ToUInt64( s.Position ) < (this.pos + this.size) )
        {
            this.children.Add( new MP4Box( br ) );
        }
    }

    private void ParseSTSD( BinaryReader br )
    {
        this.data = br.ReadBytes( 8 );
        ParseChildren( br );
    }

    private void ParseMP4A( BinaryReader br )
    {
        ParseSOUN( br );
    }

    private void ParseDRMS( BinaryReader br )
    {
        ParseSOUN( br );
    }

    private void ParseSOUN( BinaryReader br )
    {
        byte [] soun;

        soun = br.ReadBytes( 28 );

        Utility.LeReverse( soun, 8, 2 );
        if( BitConverter.ToUInt16( soun, 8 ) == 1 )
        {
            byte [] tmp = br.ReadBytes( 16 );
            byte [] esoun = new byte[ soun.Length + tmp.Length ];
            Buffer.BlockCopy( soun, 0, esoun, 0, soun.Length );
            Buffer.BlockCopy( tmp, 0, esoun, soun.Length, tmp.Length );
            soun = esoun;
        }
        Utility.LeReverse( soun, 8, 2 );

        this.data = soun;
        ParseChildren( br );
    }

    private void ParseSTSZ( BinaryReader br )
    {
        byte [] tmp;
        STSZ stsz = new STSZ();

        stsz.Dummy = br.ReadBytes( 8 );

        tmp = br.ReadBytes( 4 ); Utility.LeReverse( tmp );
        stsz.Table = new int[ BitConverter.ToInt32( tmp, 0 ) ];

        if( BitConverter.ToUInt32( stsz.Dummy, 4 ) == 0 )
        {
            for( int i = 0; i < stsz.Table.Length; i++ )
            {
                tmp = br.ReadBytes( 4 ); Utility.LeReverse( tmp );
                stsz.Table[ i ] = BitConverter.ToInt32( tmp, 0 );
            }
        }

        this.data = stsz;
    }

    private void ParseSTCO( BinaryReader br )
    {
        byte [] tmp;
        STCO stco = new STCO();

        stco.Dummy = br.ReadBytes( 4 );

        tmp = br.ReadBytes( 4 ); Utility.LeReverse( tmp );
        stco.Table = new int[ BitConverter.ToInt32( tmp, 0 ) ];

        for( int i = 0; i < stco.Table.Length; i++ )
        {
            tmp = br.ReadBytes( 4 ); Utility.LeReverse( tmp );
            stco.Table[ i ] = BitConverter.ToInt32( tmp, 0 );
        }

        this.data = stco;
    }

    private void ParseSTSC( BinaryReader br )
    {
        int Count;
        byte [] tmp;
        STSC stsc = new STSC();

        stsc.Dummy = br.ReadBytes( 4 );

        tmp = br.ReadBytes( 4 ); Utility.LeReverse( tmp );
        Count = BitConverter.ToInt32( tmp, 0 );
        stsc.FirstChunk = new int[ Count ];
        stsc.SamplesPerChunk = new int[ Count ];
        stsc.SampleDescriptionIndex = new int[ Count ];

        for( int i = 0; i < Count; i++ )
        {
            tmp = br.ReadBytes( 4 ); Utility.LeReverse( tmp );
            stsc.FirstChunk[ i ] = BitConverter.ToInt32( tmp, 0 );
            tmp = br.ReadBytes( 4 ); Utility.LeReverse( tmp );
            stsc.SamplesPerChunk[ i ] = BitConverter.ToInt32( tmp, 0 );
            tmp = br.ReadBytes( 4 ); Utility.LeReverse( tmp );
            stsc.SampleDescriptionIndex[ i ] = BitConverter.ToInt32( tmp, 0 );
        }

        this.data = stsc;
    }

    private void ParseMETA( BinaryReader br )
    {
        this.data = br.ReadBytes( 4 );
        ParseChildren( br );
    }

    public void WriteBytes( MemoryStream ms )
    {
        byte [] tmp;

        tmp = BitConverter.GetBytes( this.shortsize );
        Utility.LeReverse( tmp );
        ms.Write( tmp, 0, tmp.Length );

        tmp = BitConverter.GetBytes( this.type );
        ms.Write( tmp, 0, tmp.Length );

        if( this.shortsize == 1 )
        {
            tmp = BitConverter.GetBytes( this.size );
            Utility.LeReverse( tmp );
            ms.Write( tmp, 0, tmp.Length );
        }

        if( this.type == MP4Types.UUID )
        {
            ms.Write( this.uuid, 0, this.uuid.Length );
        }

        try
        {
            this.GetType().InvokeMember( "WriteBytes" + this.TypeStr.ToUpper(),
                BindingFlags.Default | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.Instance |
                BindingFlags.InvokeMethod, null, this,
                new object[] { ms } );
        }
        catch( MissingMethodException )
        {
            if( this.data != null )
            {
                tmp = (byte [])this.data;
                ms.Write( tmp, 0, tmp.Length );
            }
        }

        if( this.children != null )
        {
            for( int i = 0; i < this.children.Count; i++ )
            {
                MP4Box box = (MP4Box)this.children[ i ];
                box.WriteBytes( ms );
            }
        }
    }

    private void WriteBytesSTSZ( MemoryStream ms )
    {
        byte [] tmp;
        STSZ stsz = (STSZ)this.data;

        ms.Write( stsz.Dummy, 0, stsz.Dummy.Length );

        tmp = BitConverter.GetBytes( stsz.Table.Length );
        Utility.LeReverse( tmp );
        ms.Write( tmp, 0, tmp.Length );

        if( BitConverter.ToUInt32( stsz.Dummy, 4 ) == 0 )
        {
            for( int i = 0; i < stsz.Table.Length; i++ )
            {
                tmp = BitConverter.GetBytes( stsz.Table[ i ] );
                Utility.LeReverse( tmp );
                ms.Write( tmp, 0, tmp.Length );
            }
        }
    }

    private void WriteBytesSTCO( MemoryStream ms )
    {
        byte [] tmp;
        STCO stco = (STCO)this.data;

        ms.Write( stco.Dummy, 0, stco.Dummy.Length );

        tmp = BitConverter.GetBytes( stco.Table.Length );
        Utility.LeReverse( tmp );
        ms.Write( tmp, 0, tmp.Length );

        for( int i = 0; i < stco.Table.Length; i++ )
        {
            tmp = BitConverter.GetBytes( stco.Table[ i ] );
            Utility.LeReverse( tmp );
            ms.Write( tmp, 0, tmp.Length );
        }
    }

    private void WriteBytesSTSC( MemoryStream ms )
    {
        byte [] tmp;
        STSC stsc = (STSC)this.data;

        ms.Write( stsc.Dummy, 0, stsc.Dummy.Length );

        tmp = BitConverter.GetBytes( stsc.FirstChunk.Length );
        Utility.LeReverse( tmp );
        ms.Write( tmp, 0, tmp.Length );

        for( int i = 0; i < stsc.FirstChunk.Length; i++ )
        {
            tmp = BitConverter.GetBytes( stsc.FirstChunk[ i ] );
            Utility.LeReverse( tmp );
            ms.Write( tmp, 0, tmp.Length );
            tmp = BitConverter.GetBytes( stsc.SamplesPerChunk[ i ] );
            Utility.LeReverse( tmp );
            ms.Write( tmp, 0, tmp.Length );
            tmp = BitConverter.GetBytes( stsc.SampleDescriptionIndex[ i ] );
            Utility.LeReverse( tmp );
            ms.Write( tmp, 0, tmp.Length );
        }
    }

    public void Free()
    {
        if( this.type != MP4Types.FREE )
        {
            this.type = MP4Types.FREE;
            this.data = new byte[ this.size - 8 ];
            this.children = null;
        }
    }
}

public class MP4Root
{
    private ArrayList arr;

    public MP4Box this [uint index]
    {
        get
        {
            if( this.arr == null )
                return null;

            for( int i = 0; i < this.arr.Count; i++ )
            {
                MP4Box box = (MP4Box)this.arr[ i ];
                if( box.Type == index )
                    return box;
                box = box[ index ];
                if( box != null )
                    return box;
            }

            return null;
        }
    }

    public MP4Box this [uint index1, ulong index2]
    {
        get
        {
            if( this.arr == null )
                return null;

            for( int i = 0; i < this.arr.Count; i++ )
            {
                MP4Box box = (MP4Box)this.arr[ i ];
                if( (box.Type == index1) && (index2 >= box.Pos) &&
                    (index2 < (box.Pos + box.Size)) )
                    return box;
                box = box[ index1, index2 ];
                if( box != null )
                    return box;
            }

            return null;
        }
    }

    public void Parse( Stream s )
    {
        this.arr = new ArrayList();
        BinaryReader br = new BinaryReader( s );

        while( s.Position < s.Length )
        {
            this.arr.Add( new MP4Box( br ) );
        }
    }

    public byte [] Bytes
    {
        get
        {
            if( this.arr == null )
                return null;

            MemoryStream ms = new MemoryStream();

            for( int i = 0; i < this.arr.Count; i++ )
            {
                MP4Box box = (MP4Box)this.arr[ i ];
                box.WriteBytes( ms );
            }

            return ms.ToArray();
        }
    }
}
