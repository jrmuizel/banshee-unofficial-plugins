/*****************************************************************************
 * FairStore.cs: FairStore 0.3
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
using System.Net;
using System.Xml;
using System.Web;
using System.Text;
using System.Xml.XPath;
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using ICSharpCode.SharpZipLib.GZip;

public class FairStore
{
    private string strHomeDir;
    private string strDataDir;
    private Hashtable urlBag;
    private string strToken;
    private string strDSID;
    private string strGUID;
    private string strCreditBal;
    private string strFreeSongBal;
    private string strCreditDsp;
    private static NameValueCollection nvcCountries;
    private string strCountry;

    private CookieContainer signupCookies;
    private bool signupCCTypeHandled;

    public delegate void Progress( int pos, int total );

    public string DSID
    {
        get
        {
            return( strDSID );
        }
    }

    public string GUID
    {
        get
        {
            return( strGUID );
        }
    }

    public string CreditDisplay
    {
        get
        {
            if( strCreditDsp != null )
                return( strCreditDsp );
            else
                return( strCreditBal );
        }
    }

    public static NameValueCollection Countries
    {
        get
        {
            if( nvcCountries == null )
            {
                string [,] countries = new string[,]
                {
                    { "Australia", "143460" },
                    { "Austria", "143445-2" },
                    { "Belgium", "143446-2" },
                    { "Canada", "143455-6" },
                    { "Denmark", "143458" },
                    { "Finland", "143447" },
                    { "France", "143442" },
                    { "Germany", "143443" },
                    { "Great Britain", "143444" },
                    { "Greece", "143448" },
                    { "Ireland", "143449" },
                    { "Italy", "143450" },
                    { "Japan", "143462-1" },
                    { "Luxembourg", "143451-2" },
                    { "Netherlands", "143452" },
                    { "New Zealand", "143461" },
                    { "Norway", "143457" },
                    { "Portugal", "143453" },
                    { "Spain", "143454" },
                    { "Sweden", "143456" },
                    { "Switzerland", "143459" },
                    { "United States", "143441" }
                };

                nvcCountries = new NameValueCollection();
                for( int i = 0; i < countries.Length / 2; i++ )
                {
                    nvcCountries[ countries[ i, 0 ] ] = countries[ i, 1 ];
                }
            }

            return nvcCountries;
        }
    }

    public string Country
    {
        set
        {
            strCountry = value;
        }
    }

    public FairStore()
    {
        bool bUnix = Environment.OSVersion.ToString().IndexOf( "Unix" ) != -1;
        strHomeDir = bUnix ? Environment.GetEnvironmentVariable( "HOME" ) :
            Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
        strDataDir = Path.Combine( strHomeDir, bUnix ? ".drms" : "drms" );

        if( !Directory.Exists( strDataDir ) )
            Directory.CreateDirectory( strDataDir );

        string strPath = Path.Combine( strDataDir, "GUID" );

        if( File.Exists( strPath ) )
        {
            StreamReader sr = new StreamReader( strPath );
            strGUID = sr.ReadToEnd();
            sr.Close();
        }
        else
        {
            Random rnd = new Random();
            strGUID = String.Format( "{0:X8}", rnd.Next() );
            for( int i = 0; i < 6; i++ )
                strGUID += String.Format( ".{0:X8}", rnd.Next() );

            StreamWriter sw = new StreamWriter( strPath );
            sw.Write( strGUID );
            sw.Close();
        }

        urlBag = new Hashtable();
        LoadBag( false );
        LoadBag( true );
    }

    public static byte [] HexStringToBytes( string strHex )
    {
        byte [] bs = new byte[strHex.Length / 2];

        for( int i = 0; i < bs.Length; i++ )
        {
            bs[ i ] = Convert.ToByte( strHex.Substring( i * 2, 2 ), 16 );
        }

        return bs;
    }

    private Hashtable GetDict( XmlTextReader xtr )
    {
        Hashtable dict = new Hashtable();

        xtr.WhitespaceHandling = WhitespaceHandling.None;

        while( xtr.Read() )
        {
            if( xtr.NodeType == XmlNodeType.EndElement &&
                xtr.LocalName.Equals( "dict" ) )
            {
                break;
            }

            if( xtr.NodeType == XmlNodeType.Text )
            {
                int i = 0;
                string Name = null;
                string Key = xtr.Value;

                while( xtr.Read() )
                {
                    if( xtr.NodeType == XmlNodeType.Element &&
                        xtr.LocalName.Equals( "dict" ) )
                    {
                        dict[ Key ] = GetDict( xtr );
                        break;
                    }
                    else if( xtr.NodeType == XmlNodeType.Element &&
                             xtr.LocalName.Equals( "array" ) )
                    {
                        Hashtable adict;
                        ArrayList alist = new ArrayList();
                        dict[ Key ] = alist;

                        while( ( adict = GetDict( xtr ) ) != null )
                        {
                            alist.Add( adict );

                            if( !xtr.Read() )
                                break;
                            if( xtr.NodeType == XmlNodeType.EndElement &&
                                xtr.LocalName.Equals( "array" ) )
                            {
                                break;
                            }
                        }
                        break;
                    }
                    else if( xtr.NodeType == XmlNodeType.Text )
                    {
                        if( Name.Equals( "integer" ) )
                        {
                            dict[ Key ] = Int32.Parse( xtr.Value );
                        }
                        else
                        {
                            dict[ Key ] = xtr.Value;
                        }
                        break;
                    }

                    Name = xtr.LocalName;

                    if( i++ > 1 )
                    {
                        if( Name.Equals( "integer" ) )
                        {
                            dict[ Key ] = 0;
                        }
                        else
                        {
                            dict[ Key ] = String.Empty;
                        }
                        break;
                    }
                }
            }
        }

        return dict.Count > 0 ? dict : null;
    }

    private ArrayList GetDictArray( XmlTextReader xtr )
    {
        ArrayList alist = new ArrayList();

        xtr.WhitespaceHandling = WhitespaceHandling.None;

        while( xtr.Read() )
        {
            if( xtr.NodeType == XmlNodeType.Element &&
                xtr.LocalName.Equals( "array" ) )
            {
                Hashtable dict;

                while( ( dict = GetDict( xtr ) ) != null )
                {
                    alist.Add( dict );

                    if( !xtr.Read() )
                        break;
                    if( xtr.NodeType == XmlNodeType.EndElement &&
                        xtr.LocalName.Equals( "array" ) )
                    {
                        break;
                    }
                }
            }
        }

        return alist;
    }

    private string ComputeValidator( string strURL, string strUserAgent )
    {
        string strRandom = String.Format( "{0:X8}", new Random().Next() );
        byte [] abRandom = Encoding.ASCII.GetBytes( strRandom );

        byte [] abStatic =
            Convert.FromBase64String( "ROkjAaKid4EUF5kGtTNn3Q==" );

        int p = 0;
        for( int i = 0; i < 3 && p != -1; i++ )
            p = strURL.IndexOf( '/', p + 1 );

        byte [] abURL = Encoding.ASCII.GetBytes( strURL.Substring( p ) );
        byte [] abUA = Encoding.ASCII.GetBytes( strUserAgent );

        MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();
        MD5.TransformBlock( abURL, 0, abURL.Length, abURL, 0 );
        MD5.TransformBlock( abUA, 0, abUA.Length, abUA, 0 );
        MD5.TransformBlock( abStatic, 0, abStatic.Length, abStatic, 0 );
        MD5.TransformFinalBlock( abRandom, 0, abRandom.Length );

        return String.Format( "{0}-{1}", strRandom,
            BitConverter.ToString( MD5.Hash ).Replace( "-", "" ) );
    }

    private byte [] Request( string strURL, string strParams,
                             CookieContainer cookies, string strCookie,
                             byte [] postData, Progress ProgressCallback )
    {
        HttpWebRequest req =
            (HttpWebRequest)WebRequest.Create( strURL + strParams );
        req.KeepAlive = false;

        req.Headers.Add( "Accept-Language: en-us, en;q=0.50" );

        DateTime now = DateTime.Now;
        TimeSpan ts = TimeZone.CurrentTimeZone.GetUtcOffset( now );
        String strTz = String.Format( "X-Apple-Tz: {0}", ts.Hours * 3600 );
        req.Headers.Add( strTz );

        req.CookieContainer = cookies;

        if( strCookie != null )
            req.Headers.Add( "Cookie: " + strCookie );

        if( strCountry != null )
            req.Headers.Add( "X-Apple-Store-Front: " +
                             Countries[ strCountry ] );

        if( strToken != null )
            req.Headers.Add( "X-Token: " + strToken );

        req.UserAgent = "iTunes/4.7.1 (Macintosh; U; PPC Mac OS X 10.3)";

        if( strDSID != null )
            req.Headers.Add( "X-Dsid: " + strDSID );

        req.Headers.Add( "X-Apple-Validation: " +
                         ComputeValidator( strURL, req.UserAgent ) );

        req.Headers.Add( "Accept-Encoding: gzip, x-aes-cbc" );


        if( postData != null )
        {
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = postData.Length;
            Stream os = req.GetRequestStream();
            os.Write( postData, 0, postData.Length );
            os.Flush();
            os.Close();
        }

        HttpWebResponse res = (HttpWebResponse)req.GetResponse();

        byte [] content = new byte[ res.ContentLength ];
        int nread, pos = 0, len = content.Length;
        Stream rstr = res.GetResponseStream();
        while( len > 0 )
        {
            nread = rstr.Read( content, pos, len );
            if( nread > 0 )
            {
                pos += nread;
                len -= nread;
            }

            if( ProgressCallback != null )
                ProgressCallback( pos, content.Length );
        }

        if( res.ContentEncoding != null &&
            res.ContentEncoding.IndexOf( "x-aes-cbc" ) != -1 )
        {
            byte [] Key;
            Rijndael alg;

            alg = Rijndael.Create();
            alg.Mode = CipherMode.CBC;
            alg.Padding = PaddingMode.None;

            MemoryStream cms = new MemoryStream();

            if( res.Headers[ "x-apple-twofish-key" ] != null )
            {
                string strKey = res.Headers[ "x-apple-twofish-key" ];
                Key = HexStringToBytes( strKey );
                ITMS.Scramble( Key );
            }
            else
            {
                string [] Keys = new string[2]
                {
                    "ip2tOZ+wFMExvmEYINeIlQ==",
                    "mNHiLKoNir1l0UOtJ1pe5w=="
                };

                string strProKey = res.Headers[ "x-apple-protocol-key" ];
                int KeyIndex = Int32.Parse( strProKey );
                if( KeyIndex != 2 && KeyIndex != 3 )
                    throw new Exception( "Dec failed (ProKey)" );
                Key = Convert.FromBase64String( Keys[ KeyIndex - 2 ] );
            }

            string strIV = res.Headers[ "x-apple-crypto-iv" ];
            byte [] IV = HexStringToBytes( strIV );

            ICryptoTransform ct = alg.CreateDecryptor( Key, IV );
            CryptoStream cs =
                new CryptoStream( cms, ct, CryptoStreamMode.Write );
            cs.Write( content, 0, (content.Length / 16) * 16 );
            cs.Close();

            content = cms.ToArray();
        }

        if( res.ContentEncoding != null &&
            res.ContentEncoding.IndexOf( "gzip" ) != -1 )
        {
            MemoryStream gms = new MemoryStream( content, false );
            GZipInputStream gzi = new GZipInputStream( gms );
            StreamReader sr = new StreamReader( gzi );
            content = Encoding.UTF8.GetBytes( sr.ReadToEnd() );
        }

        res.Close();

        return content;
    }

    private void LoadBag( bool Secure )
    {
        string rs;
        byte [] rb;
        XmlTextReader xtr;

        string strFileName = Secure ? "secureBag.xml.gz" : "storeBag.xml.gz";
        string strPath = Path.Combine( strDataDir, strFileName );
        if( File.Exists( strPath ) )
        {
            StreamReader str = new StreamReader( strPath );
            rs = str.ReadToEnd();
            str.Close();
        }
        else
        {
            rb = Request( "http://phobos.apple.com/" + strFileName,
                          null, null, null, null, null );
            rs = Encoding.UTF8.GetString( rb );
            StreamWriter sw = new StreamWriter( strPath );
            sw.Write( rs );
            sw.Close();
        }

        xtr = new XmlTextReader( new StringReader( rs ) );
        while( xtr.Read() )
        {
            if( xtr.NodeType == XmlNodeType.Text )
            {
                if( xtr.Value.Equals( "urlBag" ) )
                {
                    Hashtable dict = GetDict( xtr );
                    IDictionaryEnumerator en = dict.GetEnumerator();
                    while( en.MoveNext() )
                    {
                        urlBag[ en.Key ] = en.Value;
                    }
                    break;
                }
            }
        }
        xtr.Close();
    }

    public void Login( string strUserID, string strPassword,
                       int AccountKind )
    {
        string rs;
        byte [] rb;
        XmlTextReader xtr;

        strDSID = null;
        strToken = null;
        strCreditBal = null;
        strFreeSongBal = null;
        strCreditDsp = null;

        if( urlBag[ "authenticateAccount" ] == null )
            throw new Exception( "authenticateAccount not in urlBag" );

        string strParams = String.Format( "?appleId={0}&password={1}" +
            "&accountKind={2}&attempt=1&guid={3}",
            HttpUtility.UrlEncode( strUserID ),
            HttpUtility.UrlEncode( strPassword ),
            AccountKind, HttpUtility.UrlEncode( strGUID ) );

        rb = Request( (string)urlBag[ "authenticateAccount" ], strParams,
                      null, null, null, null );
        rs = Encoding.UTF8.GetString( rb );

        xtr = new XmlTextReader( new StringReader( rs ) );
        Hashtable dict = GetDict( xtr );
        xtr.Close();

        if( dict == null )
            throw new Exception( "No dict in login response" );

        if( dict[ "jingleDocType" ] == null ||
            !((string)dict[ "jingleDocType" ]).Equals(
            "authenticationSuccess" ) )
        {
            throw new Exception( String.Format( "Login failed ({0})",
                (string)dict[ "customerMessage" ] ) );
        }

        strToken = (string)dict[ "passwordToken" ];
        strDSID = (string)dict[ "dsPersonId" ];
        strCreditBal = (string)dict[ "creditBalance" ];
        strFreeSongBal = (string)dict[ "freeSongBalance" ];
        strCreditDsp = (string)dict[ "creditDisplay" ];

        if( strToken == null || strDSID == null )
            throw new Exception( "Login error (Token/Dsid is null)" );
    }

    public Hashtable AuthorizeMachine()
    {
        string rs;
        byte [] rb;
        XmlTextReader xtr;

        if( urlBag[ "authorizeMachine" ] == null )
            throw new Exception( "authorizeMachine not in urlBag" );

        string strParams = String.Format( "?guid={0}",
            HttpUtility.UrlEncode( strGUID ) );

        rb = Request( (string)urlBag[ "authorizeMachine" ], strParams,
                      null, null, null, null );
        rs = Encoding.UTF8.GetString( rb );

        xtr = new XmlTextReader( new StringReader( rs ) );
        Hashtable dict = GetDict( xtr );
        xtr.Close();

        if( dict == null )
            throw new Exception( "No dict in authorize response" );

        if( dict[ "jingleDocType" ] == null ||
            !((string)dict[ "jingleDocType" ]).Equals(
            "machineAuthorizationInfoSuccess" ) )
        {
            throw new Exception( String.Format( "Authorize failed ({0})",
                (string)dict[ "customerMessage" ] ) );
        }

        dict = (Hashtable)dict[ "encryptionKeysTwofish" ];
        if( dict == null )
            throw new Exception( "No encryptionKeys in authorize response" );

        return dict;
    }

    public void DeauthorizeMachine()
    {
        string rs;
        byte [] rb;
        XmlTextReader xtr;

        if( urlBag[ "deauthorizeMachine" ] == null )
            throw new Exception( "deauthorizeMachine not in urlBag" );

        string strParams = String.Format( "?guid={0}",
            HttpUtility.UrlEncode( strGUID ) );

        rb = Request( (string)urlBag[ "deauthorizeMachine" ], strParams,
                      null, null, null, null );
        rs = Encoding.UTF8.GetString( rb );

        xtr = new XmlTextReader( new StringReader( rs ) );
        Hashtable dict = GetDict( xtr );
        xtr.Close();

        if( dict == null )
            throw new Exception( "No dict in deauthorize response" );

        if( dict[ "jingleDocType" ] == null ||
            !((string)dict[ "jingleDocType" ]).Equals(
            "success" ) )
        {
            throw new Exception( String.Format( "Deauthorize failed ({0})",
                (string)dict[ "customerMessage" ] ) );
        }
    }

    public ArrayList Search( string strTerm )
    {
        string rs;
        byte [] rb;
        XmlTextReader xtr;

        if( urlBag[ "search" ] == null )
            throw new Exception( "search not in urlBag" );

        string strParams = String.Format( "?term={0}",
            HttpUtility.UrlEncode( strTerm ) );

        rb = Request( (string)urlBag[ "search" ], strParams,
                      null, null, null, null );
        rs = Encoding.UTF8.GetString( rb );

        xtr = new XmlTextReader( new StringReader( rs ) );
        ArrayList songs = GetDictArray( xtr );
        xtr.Close();

        return songs;
    }

    public ArrayList PendingSongs()
    {
        string rs;
        byte [] rb;
        XmlTextReader xtr;

        if( urlBag[ "pendingSongs" ] == null )
            throw new Exception( "pendingSongs not in urlBag" );

        string strParams = String.Format( "?guid={0}",
            HttpUtility.UrlEncode( strGUID ) );

        rb = Request( (string)urlBag[ "pendingSongs" ], strParams,
                      null, null, null, null );
        rs = Encoding.UTF8.GetString( rb );

        xtr = new XmlTextReader( new StringReader( rs ) );
        ArrayList songs = GetDictArray( xtr );
        xtr.Close();

        return songs;
    }

    public ArrayList Buy( Hashtable song )
    {
        string rs;
        byte [] rb;
        XmlTextReader xtr;

        Hashtable meta = song[ "metaData" ] != null ?
                         (Hashtable)song[ "metaData" ] : song;

        if( urlBag[ "buyProduct" ] == null )
            throw new Exception( "buyProduct not in urlBag" );

        string strParams = String.Format( "?{0}&creditBalance={1}" +
            "&creditDisplay={2}&freeSongBalance={3}&guid={4}" +
            "&rebuy=false&buyWithoutAuthorization=true" +
            "&wasWarnedAboutFirstTimeBuy=true", meta[ "buyParams" ],
            HttpUtility.UrlEncode( strCreditBal ),
            HttpUtility.UrlEncode( strCreditDsp ),
            HttpUtility.UrlEncode( strFreeSongBal ),
            HttpUtility.UrlEncode( strGUID ) );

        rb = Request( (string)urlBag[ "buyProduct" ], strParams,
                      null, null, null, null );
        rs = Encoding.UTF8.GetString( rb );

        xtr = new XmlTextReader( new StringReader( rs ) );
        Hashtable dict = GetDict( xtr );
        xtr.Close();

        if( dict == null )
            throw new Exception( "No dict in buy response" );

        if( dict[ "jingleDocType" ] == null ||
            !((string)dict[ "jingleDocType" ]).Equals( "purchaseSuccess" ) )
        {
            throw new Exception( String.Format( "Buy failed ({0})",
                (string)dict[ "customerMessage" ] ) );
        }

        ArrayList songs = (ArrayList)dict[ "songList" ];
        if( songs == null || songs.Count < 1 )
            throw new Exception( "No songList in buy response" );

        strCreditBal = (string)dict[ "creditBalance" ];
        strFreeSongBal = (string)dict[ "freeSongBalance" ];
        strCreditDsp = (string)dict[ "creditDisplay" ];

        return songs;
    }

    public byte [] DownloadSong( Hashtable song, Progress ProgressCallback )
    {
        int i;
        Rijndael alg;
        string strDownloadKey = (string)song[ "downloadKey" ];

        if( strDownloadKey == null )
        {
            throw new Exception( "Can't download song without downloadKey" );
        }

        byte [] encData = Request( (string)song[ "URL" ], null, null,
                                   "downloadKey=" + strDownloadKey,
                                   null, ProgressCallback );

        alg = Rijndael.Create();
        alg.Mode = CipherMode.CBC;
        alg.Padding = PaddingMode.None;

        MemoryStream cms = new MemoryStream();

        byte [] ftyp = Encoding.ASCII.GetBytes( "\0\0\0\0ftypM4A \0\0\0\0" );
        cms.Write( ftyp, 0, ftyp.Length );

        byte [] Key = HexStringToBytes( (string)song[ "encryptionKey" ] );
        byte [] IV = new byte[ 16 ];
        Buffer.BlockCopy( encData, 0, IV, 0, 16 );

        ICryptoTransform ct = alg.CreateDecryptor( Key, IV );
        CryptoStream cs =
            new CryptoStream( cms, ct, CryptoStreamMode.Write );
        cs.Write( encData, 16, ((encData.Length - 16) / 16) * 16 );
        cs.Close();

        byte [] mp4 = cms.ToArray();

        byte [] moov = Encoding.ASCII.GetBytes( "moov" );

        for( i = 0; i < mp4.Length - 4; i++ )
        {
            if( mp4[ i + 0 ] == moov[ 0 ] && mp4[ i + 1 ] == moov[ 1 ] &&
                mp4[ i + 2 ] == moov[ 2 ] && mp4[ i + 3 ] == moov[ 3 ] )
            {
                byte [] offb = BitConverter.GetBytes( (UInt32)(i - 4) );
                if( BitConverter.IsLittleEndian )
                    Array.Reverse( offb, 0, 4 );
                Buffer.BlockCopy( offb, 0, mp4, 0, offb.Length );
                break;
            }
        }

        MP4Root root = new MP4Root();

        try
        {
            root.Parse( new MemoryStream( mp4 ) );
        }
        catch( InvalidBoxSizeException e )
        {
        }

        MP4Box mp4a = root[ MP4Types.MP4A ];
        if( mp4a != null )
        {
            MP4Box sinf = mp4a[ MP4Types.SINF ];
            if( sinf != null )
            {
                mp4a.Size -= sinf.Size;
                sinf.Free();
            }
        }

        return root.Bytes;
    }

    public void SongDownloadDone( Hashtable song )
    {
        string rs;
        byte [] rb;
        XmlTextReader xtr;

        if( urlBag[ "songDownloadDone" ] == null )
            throw new Exception( "songDownloadDone not in urlBag" );

        string strParams = String.Format( "?songId={0}", song[ "songId" ] );

        rb = Request( (string)urlBag[ "songDownloadDone" ], strParams,
                      null, null, null, null );
        rs = Encoding.UTF8.GetString( rb );

        xtr = new XmlTextReader( new StringReader( rs ) );
        Hashtable dict = GetDict( xtr );
        xtr.Close();

        if( dict == null )
            throw new Exception( "No dict in songDownloadDone response" );

        if( dict[ "jingleDocType" ] == null ||
            !((string)dict[ "jingleDocType" ]).Equals(
            "success" ) )
        {
            throw new Exception( String.Format( "songDownloadDone failed ({0})",
                (string)dict[ "customerMessage" ] ) );
        }
    }

    private string GetGotoURL( XmlTextReader xtr, string strViewName )
    {
        XPathDocument doc = new XPathDocument( xtr, XmlSpace.None );
        XPathNavigator nav = doc.CreateNavigator();
        XPathNodeIterator iter = nav.Select( "//*[name()=\"GotoURL\"]" );

        while( iter.MoveNext() )
        {
            string strURL = null;

            iter.Current.MoveToFirstAttribute();
            do
            {
                if( iter.Current.LocalName.Equals( "url" ) )
                {
                    strURL = iter.Current.Value;
                    break;
                }
            } while( iter.Current.MoveToNextAttribute() );

            if( strURL == null )
                continue;

            iter.Current.MoveToParent();

            if( !iter.Current.MoveToFirstChild() )
                continue;

            iter.Current.MoveToFirstAttribute();
            do
            {
                if( iter.Current.LocalName.Equals( "viewName" ) &&
                    iter.Current.Value.Equals( strViewName ) )
                {
                    return strURL;
                }
            }
            while( iter.Current.MoveToNextAttribute() );

            iter.Current.MoveToParent();
        }

        return null;
    }

    private string GetRedText( XmlTextReader xtr )
    {
        string strRed = String.Empty;

        XPathDocument doc = new XPathDocument( xtr, XmlSpace.None );
        XPathNavigator nav = doc.CreateNavigator();
        XPathNodeIterator iter = nav.Select( "//*[name()=\"TextView\" and @normalStyle=\"lucida13Red\"]" );

        while( iter.MoveNext() )
        {
            strRed += iter.Current.Value;
        }

        return strRed.Equals( String.Empty ) ? null : strRed;
    }

    public void RedeemPepsiCap( string strCode )
    {
        string rs;
        byte [] rb;
        string strURL;
        XmlTextReader xtr;
        byte [] postData;

        CookieContainer cookies = new CookieContainer();

        rb = Request( "https://phobos.apple.com/WebObjects/MZFinance.woa/wa/pepsiFreeSongWizard", null, cookies, null, null, null );
        rs = Encoding.UTF8.GetString( rb );

        xtr = new XmlTextReader( new StringReader( rs ) );
        strURL = GetGotoURL( xtr, "redeemButton" );
        xtr.Close();
        if( strURL == null )
        {
            xtr = new XmlTextReader( new StringReader( rs ) );
            Hashtable dict = GetDict( xtr );
            xtr.Close();
            if( dict != null && dict[ "customerMessage" ] != null )
                throw new Exception( (string)dict[ "customerMessage" ] );
            else
                throw new Exception( "Could not get nextURL" );
        }

        postData = Encoding.ASCII.GetBytes( "birthMonth=1&birthDay=1&birthYear=81&pepsiCanMarketSwitch=0&continueButton=submit" );
        rb = Request( "https://phobos.apple.com" + strURL,
                      null, cookies, null, postData, null );
        rs = Encoding.UTF8.GetString( rb );

        xtr = new XmlTextReader( new StringReader( rs ) );
        strURL = GetGotoURL( xtr, "redeemButton" );
        xtr.Close();
        if( strURL == null )
        {
            xtr = new XmlTextReader( new StringReader( rs ) );
            Hashtable dict = GetDict( xtr );
            xtr.Close();
            if( dict != null && dict[ "customerMessage" ] != null )
                throw new Exception( (string)dict[ "customerMessage" ] );
            else
                throw new Exception( "Could not get nextURL" );
        }

        postData = Encoding.ASCII.GetBytes( "freeProductCodeCode=" +
            HttpUtility.UrlEncode( strCode ) + "&redeemButton=submit" );
        rb = Request( "https://phobos.apple.com" + strURL,
                      null, cookies, null, postData, null );
        rs = Encoding.UTF8.GetString( rb );

        xtr = new XmlTextReader( new StringReader( rs ) );
        string strRed = GetRedText( xtr );
        xtr.Close();
        if( strRed != null )
            throw new Exception( strRed );

        strFreeSongBal =
            Convert.ToString( Int32.Parse( strFreeSongBal ) + 1000 );
    }

    public void RedeemGiftCertificate( string strCertID )
    {
        string rs;
        byte [] rb;
        XmlTextReader xtr;
        Hashtable dict = null;

        CookieContainer cookies = new CookieContainer();

        rb = Request( "https://phobos.apple.com/WebObjects/MZFinance.woa/wa/com.apple.jingle.app.finance.DirectAction/showDialogForRedeem?certId=" + HttpUtility.UrlEncode( strCertID ), null, cookies, null, null, null );
        rs = Encoding.UTF8.GetString( rb );

        xtr = new XmlTextReader( new StringReader( rs ) );
        while( xtr.Read() )
        {
            if( xtr.NodeType == XmlNodeType.Text )
            {
                if( xtr.Value.Equals( "okButtonAction" ) )
                {
                    dict = GetDict( xtr );
                    break;
                }
            }
        }
        xtr.Close();

        if( dict == null )
        {
            xtr = new XmlTextReader( new StringReader( rs ) );
            string strRed = GetRedText( xtr );
            xtr.Close();
            if( strRed != null )
                throw new Exception( strRed );
            throw new Exception( "No dict in redeem response" );
        }

        rb = Request( (string)dict[ "url" ], null, cookies, null, null, null );
        rs = Encoding.UTF8.GetString( rb );

        xtr = new XmlTextReader( new StringReader( rs ) );
        dict = GetDict( xtr );
        xtr.Close();

        if( dict == null || dict[ "creditBalance" ] == null )
            throw new Exception( "No dict in redeem response" );

        strCreditBal = (string)dict[ "creditBalance" ];
        strFreeSongBal = (string)dict[ "freeSongBalance" ];
        strCreditDsp = (string)dict[ "creditDisplay" ];
    }

    private string GetAttrValue( XmlTextReader xtr, string strName )
    {
        xtr.MoveToFirstAttribute();
        do
        {
            if( xtr.LocalName.Equals( strName ) )
                return xtr.Value;
        } while( xtr.MoveToNextAttribute() );

        return null;
    }

    private Hashtable GetForm( XmlTextReader xtr )
    {
        Hashtable form = new Hashtable();
        Hashtable data = new Hashtable();
        NameValueCollection fields = new NameValueCollection();

        xtr.WhitespaceHandling = WhitespaceHandling.None;

        while( xtr.Read() )
        {
            if( xtr.NodeType != XmlNodeType.Element )
                continue;

            if( xtr.LocalName.Equals( "TextEditView" ) ||
                xtr.LocalName.Equals( "PopupButtonView" ) ||
                xtr.LocalName.Equals( "ComboControlView" ) ||
                xtr.LocalName.Equals( "CheckboxView" ) )
            {
                Hashtable fd = new Hashtable();

                string strLocalName = xtr.LocalName;
                string strViewName = GetAttrValue( xtr, "viewName" );
                fields[ strViewName ] = strLocalName;

                if( strLocalName.Equals( "TextEditView" ) )
                {
                    string isPassword = GetAttrValue( xtr, "isPassword" );
                    if( isPassword != null )
                        fd[ "isPassword" ] = Convert.ToInt32( isPassword );
                    fd[ "value" ] = GetAttrValue( xtr, "value" );
                }
                else if( strLocalName.Equals( "PopupButtonView" ) ||
                         strLocalName.Equals( "ComboControlView" ) )
                {
                    string menuItems = GetAttrValue( xtr, "menuItems" );
                    if( menuItems != null )
                        fd[ "menuItems" ] = menuItems;
                    fd[ "value" ] = GetAttrValue( xtr, "value" );
                }
                else if( strLocalName.Equals( "CheckboxView" ) )
                {
                    fd[ "value" ] = GetAttrValue( xtr, "value" );
                }

                data[ strViewName ] = fd;
            }
        }

        form[ "fields" ] = fields;
        form[ "data" ] = data;

        return fields.Count > 0 ? form : null;
    }

    public Hashtable Signup()
    {
        string rs;
        byte [] rb;
        string strURL;
        XmlTextReader xtr;

        this.signupCookies = new CookieContainer();
        this.signupCCTypeHandled = false;

        if( urlBag[ "signup" ] == null )
            throw new Exception( "signup not in urlBag" );

        string strParams = String.Format( "?guid={0}",
            HttpUtility.UrlEncode( strGUID ) );

        rb = Request( (string)urlBag[ "signup" ], strParams,
                      this.signupCookies, null, null, null );
        rs = Encoding.UTF8.GetString( rb );

        xtr = new XmlTextReader( new StringReader( rs ) );
        strURL = GetGotoURL( xtr, "agreeButton" );
        xtr.Close();
        if( strURL == null )
            throw new Exception( "Could not get nextURL" );

        rb = Request( "https://phobos.apple.com" + strURL, null,
                      this.signupCookies, null, null, null );
        rs = Encoding.UTF8.GetString( rb );

        xtr = new XmlTextReader( new StringReader( rs ) );
        Hashtable form = GetForm( xtr );
        xtr.Close();
        if( form == null )
            throw new Exception( "Could not get form" );

        string strSubmit = "continueButton";
        xtr = new XmlTextReader( new StringReader( rs ) );
        strURL = GetGotoURL( xtr, strSubmit );
        xtr.Close();
        if( strURL == null )
            throw new Exception( "Could not get nextURL" );

        form[ "title" ] = "Account Set-up";
        form[ "submit" ] = strSubmit;
        form[ "action" ] = "post";
        form[ "url" ] = strURL;

        return form;
    }

    private NameValueCollection GetCCs( XmlTextReader xtr )
    {
        XPathDocument doc = new XPathDocument( xtr, XmlSpace.None );
        XPathNavigator nav = doc.CreateNavigator();
        XPathNodeIterator iter = nav.Select( "//*[name()=\"GotoURL\"]" );

        NameValueCollection ccs = new NameValueCollection();

        while( iter.MoveNext() )
        {
            string strURL = null;

            iter.Current.MoveToFirstAttribute();
            do
            {
                if( iter.Current.LocalName.Equals( "url" ) )
                {
                    strURL = iter.Current.Value;
                    break;
                }
            } while( iter.Current.MoveToNextAttribute() );

            if( strURL == null )
                continue;

            iter.Current.MoveToParent();

            if( !iter.Current.MoveToFirstChild() ||
                !iter.Current.MoveToNext() )
                continue;

            iter.Current.MoveToFirstAttribute();
            do
            {
                if( iter.Current.LocalName.Equals( "url" ) )
                {
                    Match m = Regex.Match( iter.Current.Value,
                                           "/cc_(.*?).png" );
                    if( m.Success )
                    {
                        string cc = m.Value.Substring( 4 ).Substring( 0,
                            m.Value.Length - 8 );
                        if( !cc.Equals( "ppal" ) )
                            ccs[ cc ] = strURL;
                    }
                }
            }
            while( iter.Current.MoveToNextAttribute() );

            iter.Current.MoveToParent();
        }

        return ccs.Count > 0 ? ccs : null;
    }

    public Hashtable SignupStep( Hashtable form )
    {
        string rs;
        byte [] rb;
        string strURL;
        XmlTextReader xtr;
        byte [] postData = null;

        NameValueCollection fields = (NameValueCollection)form[ "fields" ];

        if( form[ "urlkey" ] != null )
        {
            NameValueCollection urls = (NameValueCollection)form[ "url" ];
            strURL = urls[ fields[ (string)form[ "urlkey" ] ] ];
        }
        else
        {
            strURL = (string)form[ "url" ];
        }

        if( ((string)form[ "action" ]).Equals( "post" ) )
        {
            string strPostData = String.Empty;
            foreach( string Key in fields.AllKeys )
            {
                if( strPostData != String.Empty )
                    strPostData += "&";
                strPostData += String.Format( "{0}={1}", Key,
                               HttpUtility.UrlEncode( fields[ Key ] ) );
            }
            strPostData += String.Format( "&{0}=submit", form[ "submit" ] );
            postData = Encoding.ASCII.GetBytes( strPostData );
        }

        rb = Request( "https://phobos.apple.com" + strURL, null,
                      this.signupCookies, null, postData, null );
        rs = Encoding.UTF8.GetString( rb );

        xtr = new XmlTextReader( new StringReader( rs ) );
        NameValueCollection ccs = GetCCs( xtr );
        if( !this.signupCCTypeHandled && ccs != null )
        {
            form = new Hashtable();
            Hashtable data = new Hashtable();
            fields = new NameValueCollection();

            Hashtable fd = new Hashtable();
            string menuItems = String.Empty;
            form[ "urlkey" ] = "CreditCardType";
            fields[ "CreditCardType" ] = "PopupButtonView";
            foreach( string Key in ccs.AllKeys )
            {
                if( menuItems != String.Empty )
                    menuItems += ",";
                menuItems += Key;
            }
            fd[ "reqString" ] = true;
            fd[ "menuItems" ] = menuItems;
            data[ "CreditCardType" ] = fd;

            form[ "fields" ] = fields;
            form[ "data" ] = data;

            form[ "action" ] = "get";
            form[ "url" ] = ccs;

            this.signupCCTypeHandled = true;
        }
        else if( rs.IndexOf( "signupSuccess" ) != -1 )
        {
            form = new Hashtable();
            form[ "action" ] = "msg";
            form[ "msg" ] = "Your account has been created.";
        }
        else
        {
            string strSubmit = null;

            xtr = new XmlTextReader( new StringReader( rs ) );
            form = GetForm( xtr );
            xtr.Close();
            if( form == null )
                throw new Exception( "Could not get form" );

            string [] nextButtons = new string[]
            {
                "continueButton",
                "doneButton"
            };

            foreach( string nextButton in nextButtons )
            {
                xtr = new XmlTextReader( new StringReader( rs ) );
                strURL = GetGotoURL( xtr, nextButton );
                xtr.Close();

                if( strURL != null )
                {
                    strSubmit = nextButton;
                    break;
                }
            }
            if( strURL == null )
                throw new Exception( "Could not get nextURL" );

            xtr = new XmlTextReader( new StringReader( rs ) );
            form[ "redtext" ] = GetRedText( xtr );
            xtr.Close();

            form[ "submit" ] = strSubmit;
            form[ "action" ] = "post";
            form[ "url" ] = strURL;
        }

        form[ "title" ] = "Account Set-up";

        return form;
    }

    private string GetBuyParams( XmlTextReader xtr, string strId )
    {
        XPathDocument doc = new XPathDocument( xtr, XmlSpace.None );
        XPathNavigator nav = doc.CreateNavigator();
        XPathNodeIterator iter = nav.Select( "//*[name()=\"Buy\"]" );

        while( iter.MoveNext() )
        {
            iter.Current.MoveToFirstAttribute();
            do
            {
                if( iter.Current.LocalName.Equals( "buyParams" ) )
                {
                    if( iter.Current.Value.IndexOf( strId ) != -1 )
                        return iter.Current.Value;
                }
            } while( iter.Current.MoveToNextAttribute() );

            iter.Current.MoveToParent();
        }

        return null;
    }

    public ArrayList ViewAlbum( string strPlaylistId )
    {
        string rs;
        byte [] rb;
        XmlTextReader xtr;

        if( urlBag[ "viewAlbum" ] == null )
            throw new Exception( "search not in urlBag" );

        string strParams = String.Format( "?playlistId={0}",
            HttpUtility.UrlEncode( strPlaylistId ) );

        rb = Request( (string)urlBag[ "viewAlbum" ], strParams,
                      null, null, null, null );
        rs = Encoding.UTF8.GetString( rb );

        string strPriceDisplay = null;
        xtr = new XmlTextReader( new StringReader( rs ) );
        string strBuyParams = GetBuyParams( xtr, strPlaylistId );
        if( strBuyParams != null )
        {
            foreach( string pair in strBuyParams.Split( '&' ) )
            {
                string [] KeyValue = pair.Split( '=' );
                if( KeyValue[ 0 ].Equals( "price" ) )
                {
                    strPriceDisplay = String.Format( "{0:f} (Album)",
                        Convert.ToDouble( KeyValue[ 1 ] ) / 1000 );
                    break;
                }
            }

            if( strPriceDisplay == null )
                throw new Exception( "price not present in buyParams" );
        }

        xtr = new XmlTextReader( new StringReader( rs ) );
        ArrayList songs = GetDictArray( xtr );
        xtr.Close();

        if( strBuyParams != null )
        {
            foreach( Hashtable song in songs )
            {
                song[ "priceDisplay" ] = strPriceDisplay;
                song[ "buyParams" ] = strBuyParams;
                song[ "isAlbum" ] = true;
            }
        }

        return songs;
    }
}
