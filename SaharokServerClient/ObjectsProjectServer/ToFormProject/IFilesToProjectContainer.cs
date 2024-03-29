﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectsProjectServer
{
    public interface IFilesToProjectContainer
    {
        public FilesToPDFSort GetFilesToPDFSort();
        public string GetNameProject();

        public string GetCodeProject();

        public string GetPathProject();
    }
}
