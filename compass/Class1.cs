//绘制一条直线，startpoint：用户指出；endpoint：用户所给坐标的y轴加用户输入的长度
//复制一份直线，旋转150°，缩放为原长的4/5
//连接两条直线的endpoint
//以第一条直线为轴镜像
//画圆，以startpoint为center，radius为第一天直线长度的2/5
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Document = Autodesk.AutoCAD.ApplicationServices.Document;

namespace compass
{
    public class Class1
    {
        [CommandMethod("Compass")]
        public void Compass()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                //获取第一个点
                PromptPointResult p;
                PromptPointOptions prompt = new PromptPointOptions("");
                prompt.Message = "\n请输入第一个点:";
                prompt.AllowNone = false;
                p = doc.Editor.GetPoint(prompt);
                Point3d startPoint = p.Value;
                if (p.Status == PromptStatus.Cancel) return;

                //获取第二个点
                prompt.Message = "\n请输入第二个点:";
                prompt.BasePoint = startPoint;
                prompt.UseBasePoint = true;
                p = doc.Editor.GetPoint(prompt);
                Point3d endPoint = p.Value;
                if (p.Status == PromptStatus.Cancel) return;

                //创建对象id数组
                ObjectIdCollection oIdColl = new ObjectIdCollection();

                //画第一条线
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                using (Line l1 = new Line(startPoint, endPoint))
                {
                    btr.AppendEntity(l1);
                    trans.AddNewlyCreatedDBObject(l1, true);
                    oIdColl.Add(l1.ObjectId);
                    //复制直线
                    using (Line l2 = l1.Clone() as Line)
                    {
                        Point3d basePoint = l1.StartPoint;
                        double angle = 150.0;
                        double radian = angle * Math.PI / 180.0;
                        
                        //设置旋转轴为z轴
                        Matrix3d curUCSMatrix = doc.Editor.CurrentUserCoordinateSystem;
                        CoordinateSystem3d curUCS = curUCSMatrix.CoordinateSystem3d;
                        Vector3d axis = curUCS.Zaxis;

                        //将l2旋转150°
                        l2.TransformBy(Matrix3d.Rotation(radian, axis, basePoint));

                        //缩短为原长的4/5
                        l2.TransformBy(Matrix3d.Scaling(0.8, basePoint));
                        btr.AppendEntity(l2);
                        trans.AddNewlyCreatedDBObject(l2, true);

                        //连接两条直线的endpoint
                        using(Line l3 = new Line(l1.EndPoint, l2.EndPoint))
                        {
                            btr.AppendEntity(l3);
                            trans.AddNewlyCreatedDBObject(l3, true);

                            //镜像
                            using(Line3d lm = new Line3d(l1.StartPoint, l1.EndPoint))
                            {
                                using (Line l4 = l2.Clone() as Line)
                                {
                                    l4.TransformBy(Matrix3d.Mirroring(lm));
                                    btr.AppendEntity(l4);
                                    trans.AddNewlyCreatedDBObject(l4, true);
                                    oIdColl.Add(l4.ObjectId);
                                }

                                using (Line l5 = l3.Clone() as Line)
                                {
                                    l5.TransformBy(Matrix3d.Mirroring(lm));
                                    btr.AppendEntity(l5);
                                    trans.AddNewlyCreatedDBObject(l5, true);
                                    oIdColl.Add(l5.ObjectId);
                                }

                            }
                           
                        }
                    }

                    //画圆
                    using (Circle c = new Circle())
                    {
                        c.Center = l1.StartPoint;
                        c.Radius = l1.Length * 0.4;
                        btr.AppendEntity(c);
                        trans.AddNewlyCreatedDBObject(c, true);
                    }
                }

                //填充
                using (Hatch myHatch = new Hatch())
                {
                    btr.AppendEntity(myHatch);
                    trans.AddNewlyCreatedDBObject(myHatch, true);
                    myHatch.SetHatchPattern(HatchPatternType.PreDefined, "solid");
                    myHatch.Associative = true;
                    myHatch.AppendLoop(HatchLoopTypes.Outermost, oIdColl);
                    myHatch.HatchStyle = HatchStyle.Ignore;
                    myHatch.EvaluateHatch(true);
                }
                trans.Commit();
            }
        }
    }
}
