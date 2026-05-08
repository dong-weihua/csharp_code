//输入圆心半径，画一个十字弧
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Document = Autodesk.AutoCAD.ApplicationServices.Document;

namespace shizihu
{
    public class Class1
    {
        [CommandMethod("shizihu")]
        public void Shizihu()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            PromptPointOptions pPrompt = new PromptPointOptions("\n请输入圆心坐标：");
            PromptPointResult cCenter = doc.Editor.GetPoint(pPrompt);
            if (cCenter.Status == PromptStatus.Cancel) return;

            PromptDistanceOptions dPrompt = new PromptDistanceOptions("\n请输入半径：");
            PromptDoubleResult radius = doc.Editor.GetDistance(dPrompt);
            if (radius.Status == PromptStatus.Cancel) return;

            using(Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                //计算十字弧直线部分长度
                double length = radius.Value / 2;

                //设置三维向量，用于获取直线第二个点
                Vector3d offestLength = new Vector3d(0, length, 0);

                //获取直线第二个点
                Point3d endPoint = cCenter.Value - offestLength;

                //获取旋转单元id集合
                ObjectIdCollection rotations = new ObjectIdCollection();

                //设置旋转轴
                Matrix3d curUCSMatrix3d = doc.Editor.CurrentUserCoordinateSystem;
                CoordinateSystem3d curUCS = curUCSMatrix3d.CoordinateSystem3d;
                Vector3d axis = curUCS.Zaxis;
                                
                //画直线
                using (Line lMain =  new Line())
                {
                    lMain.StartPoint = cCenter.Value;
                    lMain.EndPoint = endPoint;

                    //克隆5份
                    for(int i = 0; i <5; i++)
                    {
                        using(Line l = lMain.Clone() as Line)
                        {
                            Vector3d lMove = new Vector3d(-5*i, 0, 0);
                                                                                
                            //每个向左移动5
                            l.TransformBy(Matrix3d.Displacement(lMove));
                            btr.AppendEntity(l);
                            trans.AddNewlyCreatedDBObject(l, true);
                            rotations.Add(l.Id);
                        }
                    }
                }

                //画4个圆弧
                for (int i = 1; i < 5; i++)
                {
                    using (Arc c = new Arc())
                    {
                        c.Center = endPoint;
                        c.Radius = 5 * i;
                        c.StartAngle = Math.PI;
                        c.EndAngle = Math.PI * 2;
                        btr.AppendEntity(c);
                        trans.AddNewlyCreatedDBObject(c, true);
                        rotations.Add(c.Id);
                    }
                }

                //环形阵列4个
                for (int i = 1; i < 4; i++)
                {
                    for (int j = 0; j < rotations.Count; j++)
                    {
                        Entity entity = trans.GetObject(rotations[j], OpenMode.ForWrite) as Entity;
                        using (Entity rotation = entity.Clone() as Entity)
                        {
                            rotation.TransformBy(Matrix3d.Rotation(Math.PI / 2 * i, axis, cCenter.Value));
                            btr.AppendEntity(rotation);
                            trans.AddNewlyCreatedDBObject(rotation, true);
                        }
                    }
                }
                trans.Commit();
            }
        }
    }
}
