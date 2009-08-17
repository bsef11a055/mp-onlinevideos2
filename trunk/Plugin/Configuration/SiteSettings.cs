﻿using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using OnlineVideos.Sites;

namespace OnlineVideos
{
    [Serializable]
    [XmlRoot("OnlineVideoSites")]
    public class SerializableSettings
    {
        SiteSettings[] sites;
        [XmlArray("Sites")]
        [XmlArrayItem("Site")]
        public SiteSettings[] Sites
        {
            get { return sites; }
            set { sites = value; }
        }
    }

    [Serializable]    
    public class SiteSettings
    {
        SiteUtilBase util;
        [XmlIgnore]
        public SiteUtilBase Util
        {
            get { return util; }
        }

        string name;
        [XmlAttribute("name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        string utilName;
        [XmlAttribute("util")]
        public string UtilName
        {
            get { return utilName; }
            set { utilName = value; util = SiteUtilFactory.GetByName(value); }
        }

        bool confirmAge;
        [XmlAttribute("agecheck")]
        public bool ConfirmAge
        {
            get { return confirmAge; }
            set { confirmAge = value; }
        }

        bool isEnabled;
        [XmlAttribute("enabled")]
        public bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; }
        }        

        string username;
        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        string password;
        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        string searchUrl;
        public string SearchUrl
        {
            get { return searchUrl; }
            set { searchUrl = value; }
        }

        [XmlAttribute("lang")]
        public string Language { get; set; }
        
        [XmlArray("Categories")]
        public Category[] CategoriesArray
        {
            get { Category[] result = new Category[Categories.Count]; Categories.Values.CopyTo(result,0); return result; }
            set { categories.Clear(); foreach (Category c in value) Categories.Add(c.Name, c); }
        }
        
        Dictionary<string, Category> categories = new Dictionary<string, Category>();
        [XmlIgnore]
        public Dictionary<string, Category> Categories
        {
            get { return categories; }
        }

        bool dynamicCategoriesDiscovered = false;
        [XmlIgnore]
        public bool DynamicCategoriesDiscovered
        {
            get { return dynamicCategoriesDiscovered; }
            set { dynamicCategoriesDiscovered = value; }
        }
        
        public override string ToString()
        {
            return Name;
        }
    }

    [Serializable]
    [XmlInclude(typeof(RssLink))]
    [XmlInclude(typeof(Group))]
    public class Category : IComparable<Category>
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("thumb")]
        public string Thumb { get; set; }
        
        #region IComparable<Category> Member

        public int CompareTo(Category other)
        {
            return Name.CompareTo(other.Name);
        }

        #endregion
    }

    [Serializable]
    public class RssLink : Category
    {        
        string url;
        [XmlText]
        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        uint estimatedVideoCount = 0;
        [XmlIgnore]
        public uint EstimatedVideoCount 
        { 
            get { return estimatedVideoCount; }
            set { estimatedVideoCount = value; } 
        }
    }

    [Serializable]
    public class Group : Category
    {
        List<Channel> channels;
        public List<Channel> Channels
        {
            get { return channels; }
            set { channels = value; }
        }
    }

    [Serializable]
    public class Channel
    {        
        [XmlAttribute("name")]
        public string StreamName { get; set; }
        
        [XmlText]
        public string Url { get; set; }

        [XmlAttribute("thumb")]
        public string Thumb { get; set; }
    }
}
