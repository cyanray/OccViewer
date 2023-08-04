#include <d3d9.h>
#include <windows.h>

// include required OCCT headers
#include <Standard_Version.hxx>
#include <Message_ProgressIndicator.hxx>
#include <Message_ProgressScope.hxx>
//for OCC graphic
#include <WNT_Window.hxx>
#include <WNT_WClass.hxx>
#include <Graphic3d_CView.hxx>
#include <Graphic3d_Camera.hxx>
#include <Graphic3d_TextureParams.hxx>
#include <D3DHost_GraphicDriver.hxx>
#include <D3DHost_View.hxx>
#include <Graphic3d_GraduatedTrihedron.hxx>
//for object display
#include <V3d_Viewer.hxx>
#include <V3d_View.hxx>
#include <AIS_InteractiveContext.hxx>
#include <AIS_Shape.hxx>
#include <AIS_RubberBand.hxx>
//topology
#include <TopoDS_Shape.hxx>
#include <TopoDS_Compound.hxx>
//brep tools
#include <BRep_Builder.hxx>
#include <BRepTools.hxx>
// iges I/E
#include <IGESControl_Reader.hxx>
#include <IGESControl_Controller.hxx>
#include <IGESControl_Writer.hxx>
#include <IFSelect_ReturnStatus.hxx>
#include <Interface_Static.hxx>
//step I/E
#include <STEPControl_Reader.hxx>
#include <STEPControl_Writer.hxx>
//for stl export
#include <StlAPI_Writer.hxx>
//for vrml export
#include <VrmlAPI_Writer.hxx>
//wrapper of pure C++ classes to ref classes
#include <NCollection_Haft.h>

#include <vcclr.h>
#include "SelectionScheme.hpp"

// list of required OCCT libraries
#pragma comment(lib, "TKernel.lib")
#pragma comment(lib, "TKMath.lib")
#pragma comment(lib, "TKBRep.lib")
#pragma comment(lib, "TKXSBase.lib")
#pragma comment(lib, "TKService.lib")
#pragma comment(lib, "TKV3d.lib")
#pragma comment(lib, "TKOpenGl.lib")
#pragma comment(lib, "TKD3dHost.lib")
#pragma comment(lib, "TKIGES.lib")
#pragma comment(lib, "TKSTEP.lib")
#pragma comment(lib, "TKStl.lib")
#pragma comment(lib, "TKVrml.lib")
#pragma comment(lib, "TKLCAF.lib")

#pragma comment(lib, "D3D9.lib")

//! Auxiliary tool for converting C# string into UTF-8 string.
static TCollection_AsciiString toAsciiString(String^ theString)
{
	if (theString == nullptr)
	{
		return TCollection_AsciiString();
	}

	pin_ptr<const wchar_t> aPinChars = PtrToStringChars(theString);
	const wchar_t* aWCharPtr = aPinChars;
	if (aWCharPtr == NULL
		|| *aWCharPtr == L'\0')
	{
		return TCollection_AsciiString();
	}
	return TCollection_AsciiString(aWCharPtr);
}

/// <summary>
/// Proxy class encapsulating calls to OCCT C++ classes within
/// C++/CLI class visible from .Net (CSharp)
/// </summary>
public ref class OCCTProxyD3D
{
public:

	OCCTProxyD3D() {}

	// ============================================
	// Viewer functionality
	// ============================================

	/// <summary>
	///Initialize a viewer
	/// </summary>
	/// <param name="theWnd">System.IntPtr that contains the window handle (HWND) of the control</param>
	bool InitViewer()
	{
		m_GraphicDriver() = new D3DHost_GraphicDriver();
		m_GraphicDriver()->ChangeOptions().buffersNoSwap = true;
		//m_GraphicDriver()->ChangeOptions().contextDebug = true;

		m_Viewer() = new V3d_Viewer(m_GraphicDriver());
		m_Viewer()->SetDefaultLights();
		m_Viewer()->SetLightOn();
		m_View() = m_Viewer()->CreateView();

		static Handle(WNT_WClass) aWClass = new WNT_WClass("OCC_Viewer", NULL, CS_OWNDC);
		Handle(WNT_Window) aWNTWindow = new WNT_Window("OCC_Viewer", aWClass, WS_POPUP, 64, 64, 64, 64);
		aWNTWindow->SetVirtual(Standard_True);
		m_View()->SetWindow(aWNTWindow);
		m_AISContext() = new AIS_InteractiveContext(m_Viewer());
		m_AISContext()->UpdateCurrentViewer();
		m_View()->MustBeResized();

		m_RubberBand() = new AIS_RubberBand();
		m_RubberBand()->SetFilling(Quantity_NOC_WHITESMOKE, 0.5);

		Handle(Prs3d_Drawer) selectionStyle = new Prs3d_Drawer();
		selectionStyle->SetLink(m_AISContext()->DefaultDrawer());
		selectionStyle->SetFaceBoundaryDraw(true);
		selectionStyle->SetDisplayMode(1); // Shaded
		selectionStyle->SetTransparency(0.4f);
		selectionStyle->SetZLayer(Graphic3d_ZLayerId_Topmost);
		selectionStyle->UnFreeBoundaryAspect()->SetWidth(1.0);
		selectionStyle->SetColor(Quantity_NOC_CYAN1);
		selectionStyle->SetBasicFillAreaAspect(new Graphic3d_AspectFillArea3d());
		const Handle(Graphic3d_AspectFillArea3d)& fillArea = selectionStyle->BasicFillAreaAspect();
		fillArea->SetColor(Quantity_NOC_GRAY);
		m_AISContext()->SetHighlightStyle(Prs3d_TypeOfHighlight_Selected, selectionStyle);

		// Display face boundary edge
		DisplayFaceBoundaryEdge(true);
		// Set MSAA samples to Anti-Aliasing
		SetMsaaSamples(8);
		// Display Triedron
		DisplayTriedron(true);
		return true;
	}

	/// <summary> Resizes custom FBO for Direct3D output. </summary>
	System::IntPtr ResizeBridgeFBO(int theWinSizeX, int theWinSizeY)
	{
		Handle(WNT_Window) aWNTWindow = Handle(WNT_Window)::DownCast(m_View()->Window());
		aWNTWindow->SetPos(0, 0, theWinSizeX, theWinSizeY);
		m_View()->MustBeResized();
		m_View()->Invalidate();
		return System::IntPtr(Handle(D3DHost_View)::DownCast(m_View()->View())->D3dColorSurface());
	}

	/// <summary>
	/// Make dump of current view to file
	/// </summary>
	/// <param name="theFileName">Name of dump file</param>
	bool Dump(const TCollection_AsciiString& theFileName)
	{
		if (m_View().IsNull())
		{
			return false;
		}
		m_View()->Redraw();
		return m_View()->Dump(theFileName.ToCString()) != Standard_False;
	}

	/// <summary>
	///Redraw view
	/// </summary>
	void RedrawView()
	{
		if (!m_View().IsNull())
		{
			m_View()->Redraw();
		}
	}

	/// <summary>
	///Update view
	/// </summary>
	void UpdateView(void)
	{
		if (!m_View().IsNull())
		{
			m_View()->MustBeResized();
		}
	}

	/// <summary>
	///Set computed mode 
	/// </summary>
	void SetDegenerateMode(bool flag)
	{
		if (!m_View().IsNull())
		{
			m_View()->SetComputedMode(flag);
			m_View()->Redraw();
		}
	}

	/// <summary>
	///Fit all
	/// </summary>
	void WindowFitAll(int theXmin, int theYmin,
		int theXmax, int theYmax)
	{
		if (!m_View().IsNull())
		{
			m_View()->WindowFitAll(theXmin, theYmin, theXmax, theYmax);
		}
	}

	/// <summary>
	///Current place of window
	/// </summary>
	/// <param name="theZoomFactor">Current zoom</param>
	void Place(int theX, int theY, float theZoomFactor)
	{
		Standard_Real aZoomFactor = theZoomFactor;
		if (!m_View().IsNull())
		{
			m_View()->Place(theX, theY, aZoomFactor);
		}
	}

	/// <summary>
	///Set Zoom
	/// </summary>
	void Zoom(int theX1, int theY1, int theX2, int theY2)
	{
		if (!m_View().IsNull())
		{
			m_View()->Zoom(theX1, theY1, theX2, theY2);
		}
	}

	/// <summary>
	///Set Pan
	/// </summary>
	void Pan(int theX, int theY)
	{
		if (!m_View().IsNull())
		{
			m_View()->Pan(theX, theY);
		}
	}

	/// <summary>
	///Rotation
	/// </summary>
	void Rotation(int theX, int theY)
	{
		if (!m_View().IsNull())
		{
			m_View()->Rotation(theX, theY);
		}
	}

	/// <summary>
	///Start rotation
	/// </summary>
	void StartRotation(int theX, int theY)
	{
		if (!m_View().IsNull())
		{
			m_View()->StartRotation(theX, theY);
		}
	}


	/// <summary>
	///Select by rectangle
	/// </summary>
	void RectangleSelection(int theX1, int theY1, int theX2, int theY2, SelectionScheme scheme)
	{
		bool partially_overlapped = (theX2 <= theX1 && theY2 >= theY1);
		m_AISContext()->MainSelector()->AllowOverlapDetection(partially_overlapped);
		if (!m_AISContext().IsNull())
		{
			m_AISContext()->SelectRectangle(Graphic3d_Vec2i(theX1, theY1),
				Graphic3d_Vec2i(theX2, theY2),
				m_View(),
				static_cast<AIS_SelectionScheme>(scheme));
			m_AISContext()->UpdateCurrentViewer();
		}
	}

	/// <summary>
	///Select by rectangle
	/// </summary>
	void RectangleSelection(int theX1, int theY1, int theX2, int theY2)
	{
		RectangleSelection(theX1, theY1, theX2, theY2, SelectionScheme::Replace);
	}

	/// <summary>
	///Select by rectanglek Xor
	/// </summary>
	void RectangleSelectionXor(int theX1, int theY1, int theX2, int theY2)
	{
		RectangleSelection(theX1, theY1, theX2, theY2, SelectionScheme::Xor);
	}

	void PickSelection(SelectionScheme scheme)
	{
		if (!m_AISContext().IsNull())
		{
			m_AISContext()->SelectDetected(static_cast<AIS_SelectionScheme>(scheme));
			m_AISContext()->UpdateCurrentViewer();
		}
	}

	/// <summary>
	/// Pick Selection
	/// </summary>
	void PickSelection()
	{
		PickSelection(SelectionScheme::Replace);
	}


	/// <summary>
	/// Pick Selection Xor
	/// </summary>
	void PickSelectionXor()
	{
		PickSelection(SelectionScheme::Xor);
	}

	/// <summary>
	///Move view
	/// </summary>
	void MoveTo(int theX, int theY)
	{
		if (!m_AISContext().IsNull() && !m_View().IsNull())
		{
			m_AISContext()->MoveTo(theX, theY, m_View(), Standard_True);
		}
	}

	/// <summary>
	///Set background color
	/// </summary>
	void BackgroundColor(int& theRed, int& theGreen, int& theBlue)
	{
		if (!m_View().IsNull())
		{
			Quantity_Color aColor = m_View()->BackgroundColor();
			theRed = (int)aColor.Red() * 255;
			theGreen = (int)aColor.Green() * 255;
			theBlue = (int)aColor.Blue() * 255;
		}
	}

	/// <summary>
	///Get background color Red
	/// </summary>
	int GetBGColR()
	{
		int anRgb[3]{};
		BackgroundColor(anRgb[0], anRgb[1], anRgb[2]);
		return anRgb[0];
	}

	/// <summary>
	///Get background color Green
	/// </summary>
	int GetBGColG()
	{
		int anRgb[3]{};
		BackgroundColor(anRgb[0], anRgb[1], anRgb[2]);
		return anRgb[1];
	}

	/// <summary>
	///Get background color Blue
	/// </summary>
	int GetBGColB()
	{
		int anRgb[3]{};
		BackgroundColor(anRgb[0], anRgb[1], anRgb[2]);
		return anRgb[2];
	}

	/// <summary>
	///Update current viewer
	/// </summary>
	void UpdateCurrentViewer()
	{
		if (!m_AISContext().IsNull())
		{
			m_AISContext()->UpdateCurrentViewer();
		}
	}

	/// <summary>
	///Front side
	/// </summary>
	void FrontView()
	{
		if (!m_View().IsNull())
		{
			m_View()->SetProj(V3d_Yneg);
		}
	}

	/// <summary>
	///Top side
	/// </summary>
	void TopView()
	{
		if (!m_View().IsNull())
		{
			m_View()->SetProj(V3d_Zpos);
		}
	}

	/// <summary>
	///Left side
	/// </summary>
	void LeftView()
	{
		if (!m_View().IsNull())
		{
			m_View()->SetProj(V3d_Xneg);
		}
	}

	/// <summary>
	///Back side
	/// </summary>
	void BackView()
	{
		if (!m_View().IsNull())
		{
			m_View()->SetProj(V3d_Ypos);
		}
	}

	/// <summary>
	///Right side
	/// </summary>
	void RightView()
	{
		if (!m_View().IsNull())
		{
			m_View()->SetProj(V3d_Xpos);
		}
	}

	/// <summary>
	///Bottom side
	/// </summary>
	void BottomView()
	{
		if (!m_View().IsNull())
		{
			m_View()->SetProj(V3d_Zneg);
		}
	}

	/// <summary>
	///Axo side
	/// </summary>
	void AxoView()
	{
		if (!m_View().IsNull())
		{
			m_View()->SetProj(V3d_XposYnegZpos);
		}
	}

	/// <summary>
	///Scale
	/// </summary>
	float Scale()
	{
		return m_View().IsNull()
			? -1.0f
			: float(m_View()->Scale());
	}

	/// <summary>
	///Zoom in all view
	/// </summary>
	void ZoomAllView()
	{
		if (!m_View().IsNull())
		{
			m_View()->FitAll();
			m_View()->ZFitAll();
		}
	}

	/// <summary>
	///Reset view
	/// </summary>
	void Reset()
	{
		if (!m_View().IsNull())
		{
			m_View()->Reset();
		}
	}

	/// <summary>
	///Set display mode of objects
	/// </summary>
	/// <param name="theMode">Set current mode</param>
	void SetDisplayMode(int theMode)
	{
		if (m_AISContext().IsNull())
		{
			return;
		}

		AIS_DisplayMode aCurrentMode = theMode == 0
			? AIS_WireFrame
			: AIS_Shaded;
		if (m_AISContext()->NbSelected() == 0)
		{
			m_AISContext()->SetDisplayMode(aCurrentMode, Standard_False);
		}
		else
		{
			for (m_AISContext()->InitSelected(); m_AISContext()->MoreSelected(); m_AISContext()->NextSelected())
			{
				m_AISContext()->SetDisplayMode(m_AISContext()->SelectedInteractive(), theMode, Standard_False);
			}
		}
		m_AISContext()->UpdateCurrentViewer();
	}

	/// <summary>
	///Set color
	/// </summary>
	void SetColor(int theR, int theG, int theB)
	{
		if (m_AISContext().IsNull())
		{
			return;
		}

		Quantity_Color aCol(theR / 255.0, theG / 255.0, theB / 255.0, Quantity_TOC_RGB);
		for (; m_AISContext()->MoreSelected(); m_AISContext()->NextSelected())
		{
			m_AISContext()->SetColor(m_AISContext()->SelectedInteractive(), aCol, false);
		}
		m_AISContext()->UpdateCurrentViewer();
	}

	/// <summary>
	///Get object color red
	/// </summary>
	int GetObjColR()
	{
		int anRgb[3]{};
		ObjectColor(anRgb[0], anRgb[1], anRgb[2]);
		return anRgb[0];
	}

	/// <summary>
	///Get object color green
	/// </summary>
	int GetObjColG()
	{
		int anRgb[3]{};
		ObjectColor(anRgb[0], anRgb[1], anRgb[2]);
		return anRgb[1];
	}

	/// <summary>
	///Get object color blue
	/// </summary>
	int GetObjColB()
	{
		int anRgb[3]{};
		ObjectColor(anRgb[0], anRgb[1], anRgb[2]);
		return anRgb[2];
	}

	/// <summary>
	///Get object color R/G/B
	/// </summary>
	void ObjectColor(int& theRed, int& theGreen, int& theBlue)
	{
		if (m_AISContext().IsNull())
		{
			return;
		}

		theRed = 255;
		theGreen = 255;
		theBlue = 255;
		m_AISContext()->InitSelected();
		if (!m_AISContext()->MoreSelected())
		{
			return;
		}

		Handle(AIS_InteractiveObject) aCurrent = m_AISContext()->SelectedInteractive();
		if (aCurrent->HasColor())
		{
			Quantity_Color anObjCol;
			m_AISContext()->Color(aCurrent, anObjCol);
			theRed = int(anObjCol.Red() * 255.0);
			theGreen = int(anObjCol.Green() * 255.0);
			theBlue = int(anObjCol.Blue() * 255.0);
		}
	}

	/// <summary>
	///Set background color R/G/B
	/// </summary>
	void SetBackgroundColor(int theRed, int theGreen, int theBlue)
	{
		if (!m_View().IsNull())
		{
			m_View()->SetBackgroundColor(Quantity_TOC_RGB, theRed / 255.0, theGreen / 255.0, theBlue / 255.0);
		}
	}

	/// <summary>
	///Erase objects
	/// </summary>
	void EraseObjects()
	{
		if (m_AISContext().IsNull())
		{
			return;
		}

		m_AISContext()->EraseSelected(Standard_False);
		m_AISContext()->ClearSelected(Standard_True);
	}

	/// <summary>
	///Get version
	/// </summary>
	float GetOCCVersion()
	{
		return (float)OCC_VERSION;
	}

	/// <summary>
	///set material
	/// </summary>
	void SetMaterial(int theMaterial)
	{
		if (m_AISContext().IsNull())
		{
			return;
		}
		for (m_AISContext()->InitSelected(); m_AISContext()->MoreSelected(); m_AISContext()->NextSelected())
		{
			m_AISContext()->SetMaterial(m_AISContext()->SelectedInteractive(), (Graphic3d_NameOfMaterial)theMaterial, Standard_False);
		}
		m_AISContext()->UpdateCurrentViewer();
	}

	/// <summary>
	///set transparency
	/// </summary>
	void SetTransparency(int theTrans)
	{
		if (m_AISContext().IsNull())
		{
			return;
		}
		for (m_AISContext()->InitSelected(); m_AISContext()->MoreSelected(); m_AISContext()->NextSelected())
		{
			m_AISContext()->SetTransparency(m_AISContext()->SelectedInteractive(), ((Standard_Real)theTrans) / 10.0, Standard_False);
		}
		m_AISContext()->UpdateCurrentViewer();
	}

	/// <summary>
	///Return true if object is selected
	/// </summary>
	bool IsObjectSelected()
	{
		if (m_AISContext().IsNull())
		{
			return false;
		}
		m_AISContext()->InitSelected();
		return m_AISContext()->MoreSelected() != Standard_False;
	}

	/// <summary>
	///Return display mode
	/// </summary>
	int DisplayMode()
	{
		if (m_AISContext().IsNull())
		{
			return -1;
		}

		bool isOneOrMoreInShading = false;
		bool isOneOrMoreInWireframe = false;
		for (m_AISContext()->InitSelected(); m_AISContext()->MoreSelected(); m_AISContext()->NextSelected())
		{
			if (m_AISContext()->IsDisplayed(m_AISContext()->SelectedInteractive(), AIS_Shaded))
			{
				isOneOrMoreInShading = true;
			}
			if (m_AISContext()->IsDisplayed(m_AISContext()->SelectedInteractive(), AIS_WireFrame))
			{
				isOneOrMoreInWireframe = true;
			}
		}
		if (isOneOrMoreInShading
			&& isOneOrMoreInWireframe)
		{
			return 10;
		}
		else if (isOneOrMoreInShading)
		{
			return 1;
		}
		else if (isOneOrMoreInWireframe)
		{
			return 0;
		}
		return -1;
	}

	/// <summary>
	///Set AISContext
	/// </summary>
	bool SetAISContext(OCCTProxyD3D^ theViewer)
	{
		this->m_AISContext() = theViewer->GetContext();
		if (m_AISContext().IsNull())
		{
			return false;
		}
		return true;
	}

	/// <summary>
	///Get AISContext
	/// </summary>
	Handle(AIS_InteractiveContext) GetContext()
	{
		return m_AISContext();
	}

	void EnableRayTracing()
	{
		m_View()->ChangeRenderingParams().Method = Graphic3d_RM_RAYTRACING;
		UpdateCurrentViewer();
	}

	void DisableRayTracing()
	{
		m_View()->ChangeRenderingParams().Method = Graphic3d_RM_RASTERIZATION;
		UpdateCurrentViewer();
	}

	void SetAntialiasing(bool theIsEnabled)
	{
		m_View()->ChangeRenderingParams().IsAntialiasingEnabled = theIsEnabled;
		UpdateCurrentViewer();
	}

	void SetRenderRation(Standard_ShortReal ration)
	{
		m_View()->ChangeRenderingParams().RenderResolutionScale = ration;
		UpdateCurrentViewer();
	}

	void DisplayFaceBoundaryEdge(bool show)
	{
		m_AISContext()->DefaultDrawer()->SetFaceBoundaryDraw(show);
		m_AISContext()->DefaultDrawer()->FaceBoundaryAspect()->SetColor(Quantity_NameOfColor::Quantity_NOC_BLACK);
		m_AISContext()->DefaultDrawer()->FaceBoundaryAspect()->SetWidth(1.0);
		UpdateCurrentViewer();
	}

	void DisplayTriedron(bool show)
	{
		if (show)
		{
			m_View()->TriedronDisplay(Aspect_TOTP_LEFT_LOWER, Quantity_NOC_SNOW, 0.1);
		}
		else
		{
			m_View()->TriedronErase();
		}
		UpdateCurrentViewer();
	}

	void SetMsaaSamples(int samples)
	{
		m_View()->ChangeRenderingParams().NbMsaaSamples = samples;
	}

	System::IntPtr GetViewPtr()
	{
		return System::IntPtr(m_View().get());
	}

	System::IntPtr GetAISContextPtr()
	{
		return System::IntPtr(m_AISContext().get());
	}

	void BeginRubberBand()
	{
		m_RubberBand()->ClearPoints();
	}

	void UpdateRubberBand(int x1, int y1, int x2, int y2)
	{
		auto filling_color = (x2 <= x1 && y2 >= y1) ? Quantity_NOC_GREEN3 : Quantity_NOC_WHITE;
		m_RubberBand()->SetFilling(filling_color, 0.5);
		m_RubberBand()->SetRectangle(x1, y1, x2, y2);
		if (!m_AISContext()->IsDisplayed(m_RubberBand()))
		{
			m_AISContext()->Display(m_RubberBand(), 0, -1, false, AIS_DS_Displayed);
		}
		else
		{
			m_AISContext()->Redisplay(m_RubberBand(), false);
		}
	}

	void EndRubberBand()
	{
		if (m_AISContext()->IsDisplayed(m_RubberBand()))
		{
			m_AISContext()->Remove(m_RubberBand(), false);
			m_RubberBand()->ClearPoints();
		}
	}

public:
	// ============================================
	// Import / export functionality
	// ============================================

	/// <summary>
	///Import BRep file
	/// </summary>
	/// <param name="theFileName">Name of import file</param>
	bool ImportBrep(System::String^ theFileName)
	{
		return ImportBrep(toAsciiString(theFileName));
	}

	/// <summary>
	///Import BRep file
	/// </summary>
	/// <param name="theFileName">Name of import file</param>
	bool ImportBrep(const TCollection_AsciiString& theFileName)
	{
		TopoDS_Shape aShape;
		BRep_Builder aBuilder;
		if (!BRepTools::Read(aShape, theFileName.ToCString(), aBuilder))
		{
			return false;
		}

		Handle(AIS_Shape) aPrs = new AIS_Shape(aShape);
		m_AISContext()->SetMaterial(aPrs, Graphic3d_NameOfMaterial_Gold, Standard_False);
		m_AISContext()->SetDisplayMode(aPrs, AIS_Shaded, Standard_False);
		m_AISContext()->Display(aPrs, Standard_True);
		return true;
	}

	/// <summary>
	///Import Step file
	/// </summary>
	/// <param name="theFileName">Name of import file</param>
	bool ImportStep(const TCollection_AsciiString& theFileName)
	{
		STEPControl_Reader aReader;
		if (aReader.ReadFile(theFileName.ToCString()) != IFSelect_RetDone)
		{
			return false;
		}

		bool isFailsonly = false;
		aReader.PrintCheckLoad(isFailsonly, IFSelect_ItemsByEntity);

		int aNbRoot = aReader.NbRootsForTransfer();
		aReader.PrintCheckTransfer(isFailsonly, IFSelect_ItemsByEntity);
		for (Standard_Integer aRootIter = 1; aRootIter <= aNbRoot; ++aRootIter)
		{
			aReader.TransferRoot(aRootIter);
			int aNbShap = aReader.NbShapes();
			if (aNbShap > 0)
			{
				for (int aShapeIter = 1; aShapeIter <= aNbShap; ++aShapeIter)
				{
					m_AISContext()->Display(new AIS_Shape(aReader.Shape(aShapeIter)), Standard_False);
				}
				m_AISContext()->UpdateCurrentViewer();
			}
		}
		return true;
	}

	/// <summary>
	///Import Iges file
	/// </summary>
	/// <param name="theFileName">Name of import file</param>
	bool ImportIges(const TCollection_AsciiString& theFileName)
	{
		IGESControl_Reader aReader;
		if (aReader.ReadFile(theFileName.ToCString()) != IFSelect_RetDone)
		{
			return false;
		}

		aReader.TransferRoots();
		TopoDS_Shape aShape = aReader.OneShape();
		m_AISContext()->Display(new AIS_Shape(aShape), Standard_False);
		m_AISContext()->UpdateCurrentViewer();
		return true;
	}

	/// <summary>
	///Export BRep file
	/// </summary>
	/// <param name="theFileName">Name of export file</param>
	bool ExportBRep(const TCollection_AsciiString& theFileName)
	{
		m_AISContext()->InitSelected();
		if (!m_AISContext()->MoreSelected())
		{
			return false;
		}

		Handle(AIS_Shape) anIS = Handle(AIS_Shape)::DownCast(m_AISContext()->SelectedInteractive());
		return !anIS.IsNull()
			&& BRepTools::Write(anIS->Shape(), theFileName.ToCString());
	}

	/// <summary>
	///Export Step file
	/// </summary>
	/// <param name="theFileName">Name of export file</param>
	bool ExportStep(const TCollection_AsciiString& theFileName)
	{
		STEPControl_StepModelType aType = STEPControl_AsIs;
		STEPControl_Writer        aWriter;
		for (m_AISContext()->InitSelected(); m_AISContext()->MoreSelected(); m_AISContext()->NextSelected())
		{
			Handle(AIS_Shape) anIS = Handle(AIS_Shape)::DownCast(m_AISContext()->SelectedInteractive());
			if (anIS.IsNull())
			{
				return false;
			}

			TopoDS_Shape aShape = anIS->Shape();
			if (aWriter.Transfer(aShape, aType) != IFSelect_RetDone)
			{
				return false;
			}
		}
		return aWriter.Write(theFileName.ToCString()) == IFSelect_RetDone;
	}

	/// <summary>
	///Export Iges file
	/// </summary>
	/// <param name="theFileName">Name of export file</param>
	bool ExportIges(const TCollection_AsciiString& theFileName)
	{
		IGESControl_Controller::Init();
		IGESControl_Writer aWriter(Interface_Static::CVal("XSTEP.iges.unit"),
			Interface_Static::IVal("XSTEP.iges.writebrep.mode"));
		for (m_AISContext()->InitSelected(); m_AISContext()->MoreSelected(); m_AISContext()->NextSelected())
		{
			Handle(AIS_Shape) anIS = Handle(AIS_Shape)::DownCast(m_AISContext()->SelectedInteractive());
			if (anIS.IsNull())
			{
				return false;
			}

			aWriter.AddShape(anIS->Shape());
		}

		aWriter.ComputeModel();
		return aWriter.Write(theFileName.ToCString()) != Standard_False;
	}

	/// <summary>
	///Export Vrml file
	/// </summary>
	/// <param name="theFileName">Name of export file</param>
	bool ExportVrml(const TCollection_AsciiString& theFileName)
	{
		TopoDS_Compound aRes;
		BRep_Builder    aBuilder;
		aBuilder.MakeCompound(aRes);
		for (m_AISContext()->InitSelected(); m_AISContext()->MoreSelected(); m_AISContext()->NextSelected())
		{
			Handle(AIS_Shape) anIS = Handle(AIS_Shape)::DownCast(m_AISContext()->SelectedInteractive());
			if (anIS.IsNull())
			{
				return false;
			}
			aBuilder.Add(aRes, anIS->Shape());
		}

		VrmlAPI_Writer aWriter;
		aWriter.Write(aRes, theFileName.ToCString());
		return true;
	}

	/// <summary>
	///Export Stl file
	/// </summary>
	/// <param name="theFileName">Name of export file</param>
	bool ExportStl(const TCollection_AsciiString& theFileName)
	{
		TopoDS_Compound aComp;
		BRep_Builder    aBuilder;
		aBuilder.MakeCompound(aComp);
		for (m_AISContext()->InitSelected(); m_AISContext()->MoreSelected(); m_AISContext()->NextSelected())
		{
			Handle(AIS_Shape) anIS = Handle(AIS_Shape)::DownCast(m_AISContext()->SelectedInteractive());
			if (anIS.IsNull())
			{
				return false;
			}
			aBuilder.Add(aComp, anIS->Shape());
		}

		StlAPI_Writer aWriter;
		aWriter.Write(aComp, theFileName.ToCString());
		return true;
	}

	/// <summary>
	///Define which Import/Export function must be called
	/// </summary>
	/// <param name="theFileName">Name of Import/Export file</param>
	/// <param name="theFormat">Determines format of Import/Export file</param>
	/// <param name="theIsImport">Determines is Import or not</param>
	bool TranslateModel(System::String^ theFileName, int theFormat, bool theIsImport)
	{
		bool  isResult = false;
		const TCollection_AsciiString aFilename = toAsciiString(theFileName);
		if (theIsImport)
		{
			switch (theFormat)
			{
			case 0: isResult = ImportBrep(aFilename); break;
			case 1: isResult = ImportStep(aFilename); break;
			case 2: isResult = ImportIges(aFilename); break;
			}
		}
		else
		{
			switch (theFormat)
			{
			case 0: isResult = ExportBRep(aFilename); break;
			case 1: isResult = ExportStep(aFilename); break;
			case 2: isResult = ExportIges(aFilename); break;
			case 3: isResult = ExportVrml(aFilename); break;
			case 4: isResult = ExportStl(aFilename); break;
			case 5: isResult = Dump(aFilename);      break;
			}
		}
		return isResult;
	}

	/// <summary>
	///Initialize OCCTProxyD3D
	/// </summary>
	void InitOCCTProxy()
	{
		m_GraphicDriver().Nullify();
		m_Viewer().Nullify();
		m_View().Nullify();
		m_AISContext().Nullify();
	}

private:

	NCollection_Haft<Handle(V3d_Viewer)>             m_Viewer;
	NCollection_Haft<Handle(V3d_View)>               m_View;
	NCollection_Haft<Handle(AIS_InteractiveContext)> m_AISContext;
	NCollection_Haft<Handle(D3DHost_GraphicDriver)>  m_GraphicDriver;
	NCollection_Haft<Handle(AIS_RubberBand)>		 m_RubberBand;

};
