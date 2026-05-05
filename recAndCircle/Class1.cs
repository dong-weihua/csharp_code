//窗花
//用户在cad中指定圆心，半径，生成一个窗花
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Document = Autodesk.AutoCAD.ApplicationServices.Document;

namespace recAndCircle
{
    public class Class1
    {
        [CommandMethod("tracery")]
        public void Tracery()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            PromptPointOptions pPrompt = new PromptPointOptions("\n请输入圆心坐标：");
            PromptPointResult point = doc.Editor.GetPoint(pPrompt);
            if (point.Status == PromptStatus.Cancel) return;

            PromptDistanceOptions dPrompt = new PromptDistanceOptions("\n请输入半径：");
            PromptDoubleResult radius = doc.Editor.GetDistance(dPrompt);
            if (radius.Status == PromptStatus.Cancel) return;

            using(Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                Point2dCollection points = new Point2dCollection();
                ObjectIdCollection lines = new ObjectIdCollection();
                Point3dCollection pointsRec = new Point3dCollection();
                Point3dCollection pointsCir = new Point3dCollection();
               
                //画主圆
                using (Circle cMain = new Circle())
                {
                    cMain.Center = point.Value;
                    cMain.Radius = radius.Value;
                    btr.AppendEntity(cMain);
                    trans.AddNewlyCreatedDBObject(cMain, true);

                    
                   
                    double partAngle = Math.PI * 2 / 6;

                    //获取多边形的每一个顶点坐标
                    for(int i=0; i<6;  i++)
                    {
                        double currentAngle = partAngle*i + Math.PI/2;
                        double x = cMain.Center.X + radius.Value*Math.Cos(currentAngle);
                        double y = cMain.Center.Y + radius.Value*Math.Sin(currentAngle);
                        Point2d interPoint = new Point2d(x, y);
                        points.Add(interPoint);
                    }

                    //画圆的内接多边形
                    using (Polyline polFour = new Polyline())
                    { 
                        for(int i=0; i<points.Count; i++)
                        {
                            polFour.AddVertexAt(i, points[i],0,0,0);
                        }
                        polFour.Closed = true;
                        btr.AppendEntity(polFour);
                        trans.AddNewlyCreatedDBObject(polFour, true);
                    }
                   

                }

                //连接多边形的顶点，形成矩形
                for (int i = 0; i < 2; i++)
                {
                    using (Line l = new Line())
                    {
                        l.StartPoint = new Point3d(points[i].X, points[i].Y, 0.0);
                        l.EndPoint = new Point3d(points[4 - i].X, points[4 - i].Y, 0.0);
                        btr.AppendEntity(l);
                        trans.AddNewlyCreatedDBObject(l, true);
                        lines.Add(l.Id);
                    }
                }
                for (int i = 2; i < 4; i++)
                {
                    using (Line l = new Line())
                    {
                        l.StartPoint = new Point3d(points[i].X, points[i].Y, 0.0);
                        if(i==2)
                            l.EndPoint = new Point3d(points[0].X, points[0].Y, 0.0);
                        else
                            l.EndPoint = new Point3d(points[5].X, points[5].Y, 0.0);
                        btr.AppendEntity(l);
                        trans.AddNewlyCreatedDBObject(l, true);
                        lines.Add (l.Id);
                    }
                }

                //从圆心处画一个米字，获取与矩形边的交点
                for(int i=0; i<4; i++)
                {
                    double currentRadius = Math.PI/4 + Math.PI/2*i;
                    double x = point.Value.X + radius.Value * Math.Cos(currentRadius);
                    double y = point.Value.Y + radius.Value * Math.Sin(currentRadius);
                    Point3d p = new Point3d(x, y, 0.0);
                    pointsCir.Add(p);
                }
                for(int i=0; i<2;  i++)
                {
                    using(Line l = new Line(pointsCir[i], pointsCir[i+2]))
                    {
                       if(i==0)
                        {
                            Line l1 = trans.GetObject(lines[i], OpenMode.ForWrite) as Line;
                            Line l2 = trans.GetObject(lines[i+1], OpenMode.ForWrite) as Line;
                            l.IntersectWith(l1, Intersect.OnBothOperands, pointsRec, 0, 0);
                            l.IntersectWith(l2, Intersect.OnBothOperands, pointsRec, 0, 0);
                        }
                       else
                        {
                            Line l1 = trans.GetObject(lines[i+1], OpenMode.ForWrite) as Line;
                            Line l2 = trans.GetObject(lines[i + 2], OpenMode.ForWrite) as Line;
                            l.IntersectWith(l1, Intersect.OnBothOperands, pointsRec, 0, 0);
                            l.IntersectWith(l2, Intersect.OnBothOperands, pointsRec, 0, 0);
                        }
                    }
                }
                
                //画正方形
                using(Polyline rec = new Polyline())
                {
                   
                    Point3d temp = pointsRec[2];
                    pointsRec[2] = pointsRec[1];
                    pointsRec[1] = temp;
                    for(int i=0;i<pointsRec.Count;i++)
                    {
                        Point2d p = new Point2d(pointsRec[i].X, pointsRec[i].Y); 
                        rec.AddVertexAt(i,p,0,0,0);
                    }
                    rec.Closed = true;
                    btr.AppendEntity(rec);
                    trans.AddNewlyCreatedDBObject(rec, true);
                }

                //画内切圆
                using(Circle ct = new Circle())
                {
                    double y = (pointsRec[0].Y + pointsRec[1].Y) / 2;
                    double rc = y - point.Value.Y;
                    ct.Center = point.Value;
                    ct.Radius = rc;
                    btr.AppendEntity(ct);
                    trans.AddNewlyCreatedDBObject(ct, true);
                }
                trans.Commit();
            }
        }
    }
}
