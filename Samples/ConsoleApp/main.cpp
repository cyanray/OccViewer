#pragma comment(lib, "OccViewer.ViewerProxy.lib")

#pragma comment(lib, "TKernel.lib")
#pragma comment(lib, "TKMath.lib")
#pragma comment(lib, "TKBRep.lib")
#pragma comment(lib, "TKXSBase.lib")
#pragma comment(lib, "TKService.lib")
#pragma comment(lib, "TKV3d.lib")
#pragma comment(lib, "TKOpenGl.lib")
#pragma comment(lib, "TKD3dHost.lib")
#pragma comment(lib, "TKLCAF.lib")
#pragma comment(lib, "TKTopAlgo.lib")
#pragma comment(lib, "TKG3d.lib")

#include <iostream>
#include <Viewer.h>
#include <Windows.h>

#include <gp_Pnt.hxx>
#include <Geom_BSplineCurve.hxx>
#include <Geom_BezierCurve.hxx>
#include <Geom_CartesianPoint.hxx>
#include <TColgp_Array1OfPnt.hxx>
#include <TColStd_Array1OfReal.hxx>
#include <TColStd_Array1OfInteger.hxx>
#include <AIS_ColoredShape.hxx>
#include <AIS_Point.hxx>
#include <AIS_InteractiveObject.hxx>
#include <AIS_InteractiveContext.hxx>
#include <BRepBuilderAPI_MakeEdge.hxx>
#include <V3d_View.hxx>

using namespace std;

using AISObjects = NCollection_Vector<Handle(AIS_InteractiveObject)>;

auto MakeCurves() -> AISObjects
{
    AISObjects myObject3d;
    // Define points.
    gp_Pnt aPnt1(0.0, 0.0, 0.0);
    gp_Pnt aPnt2(5.0, 5.0, 0.0);
    gp_Pnt aPnt3(10.0, 5.0, 0.0);
    gp_Pnt aPnt4(15.0, 0.0, 0.0);

    // Add points to the curve poles array.
    TColgp_Array1OfPnt aPoles(1, 4);
    aPoles.SetValue(1, aPnt1);
    aPoles.SetValue(2, aPnt2);
    aPoles.SetValue(3, aPnt3);
    aPoles.SetValue(4, aPnt4);

    // Define BSpline weights.
    TColStd_Array1OfReal aBSplineWeights(1, 4);
    aBSplineWeights.SetValue(1, 1.0);
    aBSplineWeights.SetValue(2, 0.5);
    aBSplineWeights.SetValue(3, 0.5);
    aBSplineWeights.SetValue(4, 1.0);

    // Define knots.
    TColStd_Array1OfReal aKnots(1, 2);
    aKnots.SetValue(1, 0.0);
    aKnots.SetValue(2, 1.0);

    // Define multiplicities.
    TColStd_Array1OfInteger aMults(1, 2);
    aMults.SetValue(1, 4);
    aMults.SetValue(2, 4);

    // Define BSpline degree and periodicity.
    Standard_Integer aDegree = 3;
    Standard_Boolean aPeriodic = Standard_False;

    // Create a BSpline curve.
    Handle(Geom_BSplineCurve) aBSplineCurve = new Geom_BSplineCurve(
        aPoles, aBSplineWeights, aKnots, aMults, aDegree, aPeriodic);
    cout << "Geom_BSplineCurve was created in red" << std::endl;

    // Define Bezier weights.
    TColStd_Array1OfReal aBezierWeights(1, 4);
    aBezierWeights.SetValue(1, 0.5);
    aBezierWeights.SetValue(2, 1.5);
    aBezierWeights.SetValue(3, 1.5);
    aBezierWeights.SetValue(4, 0.5);

    // Create Bezier curve.
    Handle(Geom_BezierCurve) aBezierCurve = new Geom_BezierCurve(aPoles, aBezierWeights);
    cout << "Geom_BezierCurve was created in green" << std::endl;

    Handle(AIS_ColoredShape) anAisBSplineCurve = new AIS_ColoredShape(
        BRepBuilderAPI_MakeEdge(aBSplineCurve).Shape());
    Handle(AIS_ColoredShape) anAisBezierCurve = new AIS_ColoredShape(
        BRepBuilderAPI_MakeEdge(aBezierCurve).Shape());
    anAisBSplineCurve->SetColor(Quantity_Color(Quantity_NOC_RED));
    anAisBezierCurve->SetColor(Quantity_Color(Quantity_NOC_GREEN));
    myObject3d.Append(anAisBSplineCurve);
    myObject3d.Append(anAisBezierCurve);
    myObject3d.Append(new AIS_Point(new Geom_CartesianPoint(aPnt1)));
    myObject3d.Append(new AIS_Point(new Geom_CartesianPoint(aPnt2)));
    myObject3d.Append(new AIS_Point(new Geom_CartesianPoint(aPnt3)));
    myObject3d.Append(new AIS_Point(new Geom_CartesianPoint(aPnt4)));
    return myObject3d;
}

Handle(V3d_View) CastViewPtr(void* ptr)
{
    return Handle(V3d_View)::handle(static_cast<V3d_View*>(ptr));
}

Handle(AIS_InteractiveContext) CastAISContextPtr(void* ptr)
{
	return Handle(AIS_InteractiveContext)::handle(static_cast<AIS_InteractiveContext*>(ptr));
}


void DisplayObjects(Handle(AIS_InteractiveContext) context, const AISObjects& objs)
{
    for (NCollection_Vector<Handle(AIS_InteractiveObject)>::Iterator anIter(objs); anIter.More(); anIter.Next())
    {
        const Handle(AIS_InteractiveObject)& anObject = anIter.Value();
        context->Display(anObject, Standard_False);
    }
}

int main()
{
    HRESULT result = CoInitialize(NULL);
    std::cout << "Hello World!\n";
    auto curves = MakeCurves();

    OccViewerProxy::Viewer viewer;
    auto view = CastViewPtr(viewer.GetViewPtr());
    auto context = CastAISContextPtr(viewer.GetAISContextPtr());
    context->RemoveAll(Standard_False);
    DisplayObjects(context, curves);
    view->Redraw();
    view->FitAll();
    viewer.ShowDialog();
}

