using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ObjectsProjectServer;
using ObjectsToFormProjectServer;

namespace SaharokServer
{
    public static class FormProject
    {
        public static FilesToPDFSort GetFilesToPDFSort(Project project)
        {

            List<SectionToProject> sectionsToProject = SectionToProject.GetSections(project);
            FilesToPDFSort filesToPDFSort = new FilesToPDFSort(sectionsToProject);
            //DoPDFFileUsingApps(filesToPDFSort);
            //CheckSectionIsDone(sectionsToProject);
            return filesToPDFSort;
        }
        public static void GetObjectsToProject(TypeDocumentation typeDocumentation)
        {
            List<SectionToProject> sectionsToProject = SectionToProject.GetSections(typeDocumentation);
            FilesToPDFSort filesToPDFSort = new FilesToPDFSort(sectionsToProject);
            //DoPDFFileUsingApps(filesToPDFSort);
            //CheckSectionIsDone(sectionsToProject);
        }
        public static void GetObjectsToProject(Section section)
        {
            SectionToProject sectionToProject = SectionToProject.GetSection(section);
            FilesToPDFSort filesToPDFSort = new FilesToPDFSort(sectionToProject);
            //DoPDFFileUsingApps(filesToPDFSort);
            //CheckSectionIsDone(sectionsToProject);
        }
        public static void GetObjectsToProject(FileSection file)
        {
            FilesToPDFSort filesToPDFSort = new FilesToPDFSort(FileToProject.GetFilesToPDFToPages(file));
            //DoPDFFileUsingApps(filesToPDFSort);
        }
    }
}
