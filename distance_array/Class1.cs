//选择一个圆弧，以该圆弧上某点为圆心画圆，通过圆与圆弧的交点，实现两圆心距离一定
//循环，直到圆弧与圆无交点
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Document = Autodesk.AutoCAD.ApplicationServices.Document;

namespace distance_array
{
    public class Class1
    {
        [CommandMethod("Disarry")]
        public void Disarry()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            //获取阵列对象
            PromptEntityOptions entityPrompt = new PromptEntityOptions("\n请选择一个阵列对象：");
            entityPrompt.SetRejectMessage("\n你选择的不是圆，请重新选择！");
            
            entityPrompt.AddAllowedClass(typeof(Circle), true);
            PromptEntityResult entityArray = doc.Editor.GetEntity(entityPrompt);
            if (entityArray.Status == PromptStatus.Cancel) return;
            //获取entity of arc/circle
            PromptEntityOptions entityPromptRoute = new PromptEntityOptions("\n请选择一个圆弧：");
            entityPromptRoute.SetRejectMessage("\n你选择的不是圆弧，请重新选择！");
            entityPromptRoute.AddAllowedClass(typeof(Arc), true);
            entityPromptRoute.AddAllowedClass(typeof(Circle), true);
            PromptEntityResult entity = doc.Editor.GetEntity(entityPromptRoute);
            if (entity.Status == PromptStatus.Cancel) return;
            PromptDistanceOptions disPrompt = new PromptDistanceOptions("\n请输入两圆心之间的距离：");
            PromptDoubleResult distance = doc.Editor.GetDistance(disPrompt);
            if(distance.Status == PromptStatus.Cancel) return;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                Curve cRoute = trans.GetObject(entity.ObjectId, OpenMode.ForRead) as Curve;
                Circle cArray = trans.GetObject(entityArray.ObjectId,OpenMode.ForWrite) as Circle;
                Point3d startCenter = cArray.Center;
                Point3dCollection intersectionPoints = new Point3dCollection();
                int count = 0;
                intersectionPoints.Add(startCenter);
                while(true)
                {
                    using(Circle c = new Circle())
                    {
                        c.Center = startCenter;
                        c.Radius = distance.Value;
                        
                        c.IntersectWith(cRoute, Intersect.OnBothOperands, intersectionPoints,0,0);
                        if (intersectionPoints.Count -(count*2+1) < 0) break;
                        if (intersectionPoints.Count -(count*2+1)== 0 )
                        {
                            if(count == 0)
                            {
                                
                                startCenter = intersectionPoints[1];
                            }

                            for (int i = 0; i < intersectionPoints.Count; i++)
                            {
                                for (int j = i + 1; j < intersectionPoints.Count; j++)
                                {
                                    if (intersectionPoints[j] != intersectionPoints[i])
                                    {
                                        startCenter = intersectionPoints[j];
                                    }
                                }
                            }
                        }
                       
                    }
                    count++;
                }
                
                for(int i=0; i<intersectionPoints.Count;  i++)
                {
                    for (int j = i + 1; j < intersectionPoints.Count; j++)
                    {
                        if (intersectionPoints[j] != intersectionPoints[i])
                        {
                            using (Circle cp = cArray.Clone() as Circle)
                            {
                                cp.Center = intersectionPoints[j];
                                btr.AppendEntity(cp);
                                trans.AddNewlyCreatedDBObject(cp, true);
                            }
                        }
                    }
                }
                trans.Commit();
            }
        }
    }
}
