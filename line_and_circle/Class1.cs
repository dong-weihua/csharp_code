using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System.Reflection.Metadata;
using Document = Autodesk.AutoCAD.ApplicationServices.Document;

namespace line_and_circle
{
    public class Class1
    {
        [CommandMethod("test")]
        public void Test()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                using (Line l1 = new Line())
                {
                    l1.StartPoint = new Point3d(0, 0, 0);
                    l1.EndPoint = new Point3d(0, 100, 0);
                    btr.AppendEntity(l1);
                    trans.AddNewlyCreatedDBObject(l1, true);
                }
                using (Circle c1 = new Circle())
                {
                    c1.Center = new Point3d(0, 50, 0);
                    c1.Radius = 50;
                    btr.AppendEntity(c1);
                    trans.AddNewlyCreatedDBObject(c1, true);
                }
                trans.Commit();
            }
        }
    }
}
