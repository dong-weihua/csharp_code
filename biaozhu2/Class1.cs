using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace biaozhu2
{
    public class Class1
    {
        [CommandMethod("biaozhu")]
        public void Biaozhu()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            PromptPointOptions pPrompt = new PromptPointOptions("\n请输入第一个点：")
            {
                AllowNone = false
            };
            PromptPointResult pStart = doc.Editor.GetPoint(pPrompt);
            if (pStart.Status == PromptStatus.Cancel) return;

            pPrompt.UseBasePoint = true;
            pPrompt.BasePoint = pStart.Value;

            pPrompt.Message = "\n请输入第二个点：";
            PromptPointResult pEnd = doc.Editor.GetPoint(pPrompt);
            if (pEnd.Status == PromptStatus.Cancel) return;

            using(Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable? bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord? btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                Point3d pText = new Point3d((pStart.Value.X+pEnd.Value.X)/2, pStart.Value.Y+20, 0);
                double rotation = 0;
                string dimText = "";
                RotatedDimension rotDim = new RotatedDimension();
                rotDim.XLine1Point = pStart.Value;
                rotDim.XLine2Point = pEnd.Value;
                rotDim.DimLinePoint = pText;
                rotDim.Rotation = rotation;
                rotDim.DimensionText = dimText;
                rotDim.DimensionStyle = db.Dimstyle;
                btr.AppendEntity(rotDim);
                trans.AddNewlyCreatedDBObject(rotDim, true);
                trans.Commit();
            }
        }
    }
}
