﻿namespace ZDFMediathek2009.Code.DTO
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Xml.Serialization;

    [Serializable, XmlType(AnonymousType=true), XmlRoot(Namespace="", IsNullable=false), DebuggerStepThrough, DesignerCategory("code"), GeneratedCode("xsd", "2.0.50727.3038")]
    public class mainnavigation
    {
        private mainnavigationMainnaviitem[] mainnaviitemField;

        [XmlElement("mainnaviitem")]
        public mainnavigationMainnaviitem[] mainnaviitem
        {
            get
            {
                return this.mainnaviitemField;
            }
            set
            {
                this.mainnaviitemField = value;
            }
        }
    }
}

