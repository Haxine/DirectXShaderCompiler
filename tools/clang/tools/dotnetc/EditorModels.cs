﻿///////////////////////////////////////////////////////////////////////////////
//                                                                           //
// EditorModels.cs                                                           //
// Copyright (C) Microsoft Corporation. All rights reserved.                 //
// Licensed under the MIT license. See COPYRIGHT in the project root for     //
// full license information.                                                 //
//                                                                           //
// Provides support for model classes used by the editor UI.                 //
//                                                                           //
///////////////////////////////////////////////////////////////////////////////

using DotNetDxc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace MainNs
{
    public class DiagnosticDetail
    {
        [DisplayName("Error")]
        public int ErrorCode { get; set; }
        [DisplayName("Line")]
        public int ErrorLine { get; set; }
        [DisplayName("Column")]
        public int ErrorColumn { get; set; }
        [DisplayName("File")]
        public string ErrorFile { get; set; }
        [DisplayName("Offset")]
        public int ErrorOffset { get; set; }
        [DisplayName("Length")]
        public int ErrorLength { get; set; }
        [DisplayName("Message")]
        public string ErrorMessage { get; set; }
    }

    [DebuggerDisplay("{Name}")]
    class PassArgInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public PassInfo PassInfo { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }

    [DebuggerDisplay("{Arg.Name} = {Value}")]
    class PassArgValueInfo
    {
        public PassArgInfo Arg { get; set; }
        public string Value { get; set; }
        public override string ToString()
        {
            if (String.IsNullOrEmpty(Value))
                return Arg.Name;
            return Arg.Name + "=" + Value;
        }
    }

    [DebuggerDisplay("{Name}")]
    class PassInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public PassArgInfo[] Args { get; set; }
        public static PassInfo FromOptimizerPass(IDxcOptimizerPass pass)
        {
            PassInfo result = new PassInfo()
            {
                Name = pass.GetOptionName(),
                Description = pass.GetDescription()
            };
            PassArgInfo[] args = new PassArgInfo[pass.GetOptionArgCount()];
            for (int i = 0; i < pass.GetOptionArgCount(); ++i)
            {
                PassArgInfo info = new PassArgInfo()
                {
                    Name = pass.GetOptionArgName((uint)i),
                    Description = pass.GetOptionArgDescription((uint)i),
                    PassInfo = result
                };
                args[i] = info;
            }
            result.Args = args;
            return result;
        }
        public override string ToString()
        {
            return Name;
        }
    }

    class PassInfoWithValues
    {
        public PassInfoWithValues(PassInfo pass)
        {
            this.PassInfo = pass;
            this.Values = new List<PassArgValueInfo>();
        }
        public PassInfo PassInfo { get; set; }
        public List<PassArgValueInfo> Values { get; set; }
        public override string ToString()
        {
            string result = this.PassInfo.Name;
            if (this.Values.Count == 0)
                return result;
            result += String.Concat(this.Values.Select(v => "," + v.ToString()));
            return result;
        }
    }

    class MRUManager
    {
        #region Private fields.

        private List<string> MRUFiles = new List<string>();

        #endregion Private fields.

        #region Constructors.

        public MRUManager()
        {
            this.MaxCount = 8;
            this.MRUPath =
                System.IO.Path.Combine(
                    System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "dndxc",
                    "mru.txt");
        }

        #endregion Constructors.

        #region Public properties.

        public int MaxCount { get; set; }

        public string MRUPath { get; set; }

        public IEnumerable<string> Paths
        {
            get { return this.MRUFiles; }
        }

        #endregion Public properties.

        #region Public methods.

        public void LoadFromFile()
        {
            this.LoadFromFile(this.MRUPath);
        }

        public void LoadFromFile(string path)
        {
            if (!System.IO.File.Exists(path))
                return;
            this.MRUFiles = System.IO.File.ReadAllLines(path).ToList();
        }

        public void SaveToFile()
        {
            this.SaveToFile(this.MRUPath);
        }

        public void SaveToFile(string path)
        {
            string dirName = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(dirName))
                System.IO.Directory.CreateDirectory(dirName);
            System.IO.File.WriteAllLines(path, this.MRUFiles);
        }

        public void HandleFileLoad(string path)
        {
            this.HandleFileSave(path);
        }

        public void HandleFileSave(string path)
        {
            path = System.IO.Path.GetFullPath(path);
            int index = this.MRUFiles.IndexOf(path);
            if (index >= 0)
                this.MRUFiles.RemoveAt(index);
            this.MRUFiles.Insert(0, path);
            while (this.MRUFiles.Count > this.MaxCount)
                this.MRUFiles.RemoveAt(this.MRUFiles.Count - 1);
        }

        public void HandleFileFail(string path)
        {
            path = System.IO.Path.GetFullPath(path);
            int index = this.MRUFiles.IndexOf(path);
            if (index >= 0)
                this.MRUFiles.RemoveAt(index);
        }

        #endregion Public methods.
    }
}
