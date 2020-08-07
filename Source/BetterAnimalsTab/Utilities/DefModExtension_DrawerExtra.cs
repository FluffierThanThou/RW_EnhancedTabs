﻿// DefModExtension_DrawerExtra.cs
// Copyright Karel Kroeze, 2020-2020

using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace AnimalTab
{
    public class DefModExtension_DrawerExtra: DefModExtension
    {
        public Type workerType;
        private DrawWorker _worker;

        public DrawWorker Worker
        {
            get { return _worker ??= (DrawWorker) Activator.CreateInstance( workerType ); }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach ( var error in base.ConfigErrors() )
                yield return error;
            if ( workerType == null )
                yield return $"No draw worker set!";
        }
    }

    public abstract class DrawWorker
    {
        public abstract int Draw( Rect tabArea );
    }
}