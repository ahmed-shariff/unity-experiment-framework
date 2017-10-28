﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExpMngr
{
    public class TextFormController : FormElementController
    {
        public Text placeholder;
        public InputField inputField;



        public override object GetContents()
        {
            switch (dataType)
            {
                case FormDataType.Float:
                    return float.Parse(inputField.text);
                case FormDataType.Int:
                    return int.Parse(inputField.text);
                case FormDataType.String:
                    return inputField.text;
                default:
                    throw new System.Exception("Datatype undefined");
            }
        }

        public override void SetContents(object newContents)
        {
            inputField.text = (string) newContents;
        }

        public override void Clear()
        {
            inputField.text = "";
        }

        protected override void Setup()
        {
            
            string pText;
            switch (dataType)
            {
                case FormDataType.Float:
                    pText = string.Format("Enter number... [float]");
                    break;
                case FormDataType.Int:
                    pText = string.Format("Enter number... [int]");
                    break;
                case FormDataType.String:
                    pText = string.Format("Enter text... [string]");
                    break;
                default:
                    throw new System.Exception("Datatype undefined");
            }
            placeholder.text = pText;
        }

    }
}