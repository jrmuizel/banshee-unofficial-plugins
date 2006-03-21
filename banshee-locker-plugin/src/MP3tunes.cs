/*
 * Copyright (C) 2005-2006 MP3tunes, LLC
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using System.Text;
using System.Collections;
using System.Globalization;

namespace MP3tunes
{
    public class Track
    {
        private Uri uri;
        private string artist;
        private string album;
        private string title;
        private int year;
        private uint track_number;
        private TimeSpan duration;

        public Track()
        {

        }

        public Uri Uri
        {
            get { return uri; }
            set { uri = value; }
        }

        public string Artist
        {
            get { return artist; }
            set { artist = value; }
        }

        public string Album
        {
            get { return album; }
            set { album = value; }
        }

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        public int Year
        {
            get { return year; }
            set { year = value; }
        }

        public TimeSpan Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        public uint TrackNumber
        {
            get { return track_number; }
            set { track_number = value; }
        }
    }

    public class AuthenticationException : ApplicationException
    {
        public AuthenticationException( string msg ) : base( msg )
        {

        }
    }
    
    public class LockerCertificatePolicy : ICertificatePolicy
    {
        public bool CheckValidationResult(ServicePoint servicePoint, 
            System.Security.Cryptography.X509Certificates.X509Certificate certificate, 
            WebRequest webRequest, int certificateProblem)
        {
            return true;
        }
    }

    public class Locker
    {
        private string session_id;
        private string partner_token;

        private LockerCertificatePolicy policy;

        public string PartnerToken
        {
            get { return this.partner_token; }
            set { this.partner_token = value; }
        }

        public Locker( string _partner_token )
        {
            this.partner_token = _partner_token;
        }

        string Request( string strURL, string strParams )
        {
            if(policy == null) {
                policy = new LockerCertificatePolicy();
                ServicePointManager.CertificatePolicy = policy;
            }
        
            HttpWebRequest req =
                (HttpWebRequest)WebRequest.Create( strURL + strParams );
            req.KeepAlive = false;

            req.UserAgent = "MP3tunes.NET 1.0";

            HttpWebResponse res = (HttpWebResponse)req.GetResponse();

            Stream rstr = res.GetResponseStream();
            StreamReader reader = new StreamReader( rstr );
            string content = reader.ReadToEnd();

            res.Close();

            return content;
        }

        public void Login( string username, string password )
        {
            string rs;
            XmlNode status;
            XmlNode session_id;
            XmlNode errorMessage;
            XmlDocument doc = new XmlDocument();

            string strParams = String.Format( "?output=xml&username={0}" +
                "&password={1}", HttpUtility.UrlEncode( username ),
                HttpUtility.UrlEncode( password ) );

            rs = Request( "https://shop.mp3tunes.com/api/v0/login", strParams );
            doc.LoadXml( rs );

            status = doc.SelectSingleNode( "mp3tunes/status" );
            session_id = doc.SelectSingleNode( "mp3tunes/session_id" );
            errorMessage = doc.SelectSingleNode( "mp3tunes/errorMessage" );

            if( status != null && status.InnerXml == "1" &&
                session_id != null && session_id.InnerXml != "null" )
            {
                this.session_id = session_id.InnerXml;
            }
            else if( errorMessage != null && errorMessage.InnerXml != "null" )
            {
                throw new AuthenticationException( (string)
                                                   errorMessage.InnerXml );
            }
            else
            {
                throw new AuthenticationException( "Wrong username or " +
                                                   "password." );
            }
        }

        public ArrayList GetTracks()
        {
            string rs;
            ArrayList alist = new ArrayList();
            XmlDocument doc = new XmlDocument();

            string strParams = String.Format( "?output=xml&sid={0}&type=track",
                HttpUtility.UrlEncode( this.session_id ) );

            rs = Request( "http://www.mp3tunes.com/api/v0/lockerData",
                          strParams );

            doc.LoadXml( rs );
            XPathNavigator nav = doc.CreateNavigator();
            XPathNodeIterator iter = nav.Select( "/mp3tunes/trackList/item" );

            while( iter.MoveNext() )
            {
                XmlNode node = ((IHasXmlNode)iter.Current).GetNode();
                //XmlNode trackId = node.SelectSingleNode( "trackId" );
                XmlNode trackTitle = node.SelectSingleNode( "trackTitle" );
                XmlNode trackNumber = node.SelectSingleNode( "trackNumber" );
                XmlNode trackLength = node.SelectSingleNode( "trackLength" );
                //XmlNode trackFileName = node.SelectSingleNode( "trackFileName" );
                //XmlNode trackFileKey = node.SelectSingleNode( "trackFileKey" );
                XmlNode downloadURL = node.SelectSingleNode( "downloadURL" );
                XmlNode albumTitle = node.SelectSingleNode( "albumTitle" );
                //XmlNode albumYear = node.SelectSingleNode( "albumYear" );
                XmlNode artistName = node.SelectSingleNode( "artistName" );

                Track tr = new Track();
                tr.Uri = new Uri( downloadURL.InnerXml );
                tr.Artist = artistName.InnerXml;
                tr.Album = albumTitle.InnerXml;
                tr.Title = trackTitle.InnerXml;
                tr.Duration = new TimeSpan( Convert.ToInt64( Convert.ToDouble( trackLength.InnerXml ) ) * 10000 );
                if( trackNumber != null &&
                    trackNumber.InnerXml != "" )
                {
                    tr.TrackNumber = Convert.ToUInt32( trackNumber.InnerXml );
                }
                alist.Add( tr );
            }

            return alist;
        }
    }
}
