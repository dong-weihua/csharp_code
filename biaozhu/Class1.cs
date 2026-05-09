using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace biaozhu
{
    public class Class1
    {
        [CommandMethod("biaozhu")]
        public void Biaozhu()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            PromptPointOptions pPrompt = new PromptPointOptions("\n请输入圆心坐标：");
            PromptPointResult cCenter = doc.Editor.GetPoint(pPrompt);
            if (cCenter.Status == PromptStatus.Cancel) return;

            PromptDistanceOptions dPrompt = new PromptDistanceOptions("\n请输入半径：");
            PromptDoubleResult radius = doc.Editor.GetDistance(dPrompt);
            if (radius.Status == PromptStatus.Cancel) return;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                Vector3d offsetP = new Vector3d(0, radius.Value/2, 0);
                Point3d endPoint = cCenter  .Value + offsetP;

                ObjectIdCollection rotation1 = new ObjectIdCollection();

                ObjectIdCollection rotation2 = new ObjectIdCollection();

                //设置旋转轴
                Matrix3d curUCSMatrix3d = doc.Editor.CurrentUserCoordinateSystem;
                CoordinateSystem3d curUCS = curUCSMatrix3d.CoordinateSystem3d;
                Vector3d axis = curUCS.Zaxis;

                Point3d center = new Point3d();

                using (Line lMain = new Line(cCenter.Value, endPoint))
                {
                    for(int i = 0; i < 4; i++)
                    {
                        using(Line l = lMain.Clone() as Line)
                        {
                            Vector3d move = new Vector3d(radius.Value/6*i, 0, 0);
                            l.TransformBy(Matrix3d.Displacement(move));
                            btr.AppendEntity(l);
                            trans.AddNewlyCreatedDBObject(l, true);
                            rotation1.Add(l.Id);
                            if(i == 3)
                            {
                                Line lUp = new Line(lMain.StartPoint, l.StartPoint);
                                Line lDown = new Line(lMain.EndPoint, l.EndPoint);
                                center = l.EndPoint;
                                btr.AppendEntity(lUp);
                                trans.AddNewlyCreatedDBObject(lUp, true);
                                rotation1.Add(lUp.Id);
                                btr.AppendEntity(lDown);
                                trans.AddNewlyCreatedDBObject(lDown, true);
                                rotation1.Add(lDown.Id);
                            }
                        }
                    }

                    //内旋转
                    for(int i = 1;i < 4;i++)
                    {
                        for (int j = 0; j < rotation1.Count; j++)
                        {
                            Entity entity = trans.GetObject(rotation1[j], OpenMode.ForWrite) as Entity;
                          
                            rotation2.Add(entity.Id);
                            using (Entity roEntity = entity.Clone()as Entity)
                            {
                                roEntity.TransformBy(Matrix3d.Rotation(Math.PI / 2 * i, axis, center));
                                btr.AppendEntity(roEntity);
                                trans.AddNewlyCreatedDBObject(roEntity, true);
                                rotation2.Add(roEntity.Id);
                            }
                        }
                    }

                    //外旋转
                    for (int i = 1; i < 4; i++)
                    {
                        for (int j = 0; j < rotation2.Count; j++)
                        {
                            Entity entity = trans.GetObject(rotation2[j], OpenMode.ForWrite) as Entity;

                            using (Entity roEntity = entity.Clone() as Entity)
                            {
                                roEntity.TransformBy(Matrix3d.Rotation(Math.PI / 2 * i, axis, cCenter.Value));
                                btr.AppendEntity(roEntity);
                                trans.AddNewlyCreatedDBObject(roEntity, true);                               
                            }
                        }
                    }


                }
                trans.Commit();
            }
        }
    }
}
