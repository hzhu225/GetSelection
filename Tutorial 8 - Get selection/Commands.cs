// Written by Felix Zhu @ CMS Surveyors

/********************************************************************************************************************************************
     In this tutorial, we will learn how to get user selection and access each selected entity.
     Users will be able to select multiple Lines and 3D Polylines.
     For selected Lines, its length will be printed. For selected 3D Polylines, the StartPoint will be printed.
     You don't need to write any code in this tutorial.
*********************************************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;

namespace Tutorial_8
{
    public class Commands
    {
        [CommandMethod("Tut8")]
        public void Tutorial_8_Get_selection()
        {
            CivilDocument doc = CivilApplication.ActiveDocument;
            Document MdiActdoc = Application.DocumentManager.MdiActiveDocument;
            Editor editor = MdiActdoc.Editor;
            Database currentDB = MdiActdoc.Database;

            //Selection is also a kind of "user input". There are PromptSelectionOption and PromptSelectionResult.

            //However, in most case only certain type of objects should be selected. Hence we need a SelectionFilter. The constuctor of SelectionFilter needs a TypedValue[].

            TypedValue[] filter_typedValue = new TypedValue[] {               //an array of TypedValue
                new TypedValue((int)DxfCode.Operator, "<or"),
                new TypedValue((int)DxfCode.Start, "LINE"),
                new TypedValue((int)DxfCode.Start, "POLYLINE"),               //"POLYLINE" is type for 3D Polyline, use LIST command in Civil3D to get this name.
                new TypedValue((int)DxfCode.Operator, "or>")
                };

            SelectionFilter myFilter = new SelectionFilter(filter_typedValue);           //With this filter, only line or 3D polyline can be selected.       


            PromptSelectionOptions pso = new PromptSelectionOptions();         //Althouth we had SelectionFilter already, we can still have PromptSelectionOption
            pso.MessageForAdding = "\nSelect lines or 3D polylines: ";        //Add prompt message through PromptSelectionOption. You can type "pso." and change other selecting options.


            PromptSelectionResult psr = editor.GetSelection(pso, myFilter);        //editor.GetSelection can take both PromptSelectionOptions and SelectionFilter as parameter.


            using (Transaction trans = HostApplicationServices.WorkingDatabase.TransactionManager.StartOpenCloseTransaction())
            {
                //We don't need to access modelSpace because nothing in modelSpace will be changed.

                if (psr.Status == PromptStatus.OK)                               //check psr.Status in case user pressing ESC
                {
                    ObjectIdCollection selectIdCol = new ObjectIdCollection(psr.Value.GetObjectIds());      //because user can select multiple objects, the Value is an objectIdCollection

                    foreach (ObjectId selectId in selectIdCol)                                       //use "foreach" to go through each objectId
                    {
                        try
                        {
                            object obj = trans.GetObject(selectId, OpenMode.ForRead);     //get object from objectId
                                                                                          //because not sure it is a line or 3D polyline, get it as a general type "object" first.

                            string typename = obj.GetType().Name;                           //get the type name of object

                            if (typename == "Line")                       // if obj is a Line
                            {
                                Line myline = obj as Line;                 //transfer type from object to Line
                                editor.WriteMessage("\nYou picked a line which length is: " + myline.Length);
                            }

                            else if (typename == "Polyline3d")              //if obj is a 3D polyline.  Think: Can we use if here instead of else if? what is the difference?
                            {
                                Polyline3d mypoly = obj as Polyline3d;
                                editor.WriteMessage("\nYou picked a 3D polyline with StartPoint: " + mypoly.StartPoint);
                            }

                        }
                        catch (Autodesk.AutoCAD.Runtime.Exception ex)                      //catch any exceptions
                        {
                            editor.WriteMessage("\nError: " + ex.Message);
                        }
                    }
                }

                trans.Commit();
            }
        }
    }
}

//Build, load and run "Tut8" command in Civil3D.


