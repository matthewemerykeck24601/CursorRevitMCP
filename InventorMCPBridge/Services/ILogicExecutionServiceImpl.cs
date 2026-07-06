using Inventor;
using System.Linq;

namespace InventorMCPBridge.Services;

public sealed class ILogicExecutionServiceImpl : ILogicExecutionService
{
    private readonly Inventor.Application _app;

    public ILogicExecutionServiceImpl(Inventor.Application app)
    {
        _app = app;
    }

    public object GetActiveDocumentInfo()
    {
        dynamic document = _app.ActiveDocument;
        if (document is null)
        {
            throw new InvalidOperationException("No active Inventor document.");
        }

        return new
        {
            displayName = document.DisplayName,
            fullFileName = document.FullFileName,
            documentType = document.DocumentType.ToString(),
            dirty = document.Dirty,
        };
    }

    public object RunRule(string ruleName, string runMode)
    {
        if (string.IsNullOrWhiteSpace(ruleName))
        {
            throw new InvalidOperationException("Rule name is required.");
        }

        var automation = TryGetILogicAutomation()
            ?? throw new InvalidOperationException("iLogic automation is unavailable in this Inventor session.");

        dynamic doc = _app.ActiveDocument
            ?? throw new InvalidOperationException("No active Inventor document.");

        try
        {
            if (runMode.Equals("allReferenced", StringComparison.OrdinalIgnoreCase))
            {
                automation.RunExternalRuleWithArguments(doc, ruleName, null);
            }
            else
            {
                automation.RunRule(doc, ruleName);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to run iLogic rule '{ruleName}': {ex.Message}", ex);
        }

        return new
        {
            success = true,
            ruleName,
            runMode,
            documentDirty = doc.Dirty,
        };
    }

    public object ShowForm(string formName)
    {
        if (string.IsNullOrWhiteSpace(formName))
        {
            throw new InvalidOperationException("Form name is required.");
        }

        var automation = TryGetILogicAutomation()
            ?? throw new InvalidOperationException("iLogic automation is unavailable in this Inventor session.");

        dynamic doc = _app.ActiveDocument
            ?? throw new InvalidOperationException("No active Inventor document.");

        var trimmed = formName.Trim();
        string? methodUsed = null;
        var shown =
            TryInvokeAutomation(automation, "ShowForm", out methodUsed, doc, trimmed) ||
            TryInvokeAutomation(automation, "ShowForm", out methodUsed, trimmed) ||
            TryInvokeAutomation(automation, "ShowiLogicForm", out methodUsed, doc, trimmed) ||
            TryInvokeAutomation(automation, "ShowiLogicForm", out methodUsed, trimmed);

        if (!shown)
        {
            throw new InvalidOperationException(
                $"Failed to show iLogic form '{trimmed}'. Ensure a form with that name exists in the active document.");
        }

        return new
        {
            success = true,
            formName = trimmed,
            method = methodUsed ?? "unknown",
            documentDirty = doc.Dirty,
        };
    }

    public object WriteRule(string ruleName, string code, bool overwrite)
    {
        if (string.IsNullOrWhiteSpace(ruleName))
        {
            throw new InvalidOperationException("Rule name is required.");
        }
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new InvalidOperationException("Rule code is required.");
        }

        var automation = TryGetILogicAutomation()
            ?? throw new InvalidOperationException("iLogic automation is unavailable in this Inventor session.");

        dynamic doc = _app.ActiveDocument
            ?? throw new InvalidOperationException("No active Inventor document.");

        try
        {
            if (overwrite)
            {
                automation.DeleteRule(doc, ruleName);
            }

            automation.AddRule(doc, ruleName, code);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to write iLogic rule '{ruleName}': {ex.Message}", ex);
        }

        return new
        {
            success = true,
            ruleName,
            overwrite,
            documentDirty = doc.Dirty,
        };
    }

    public List<ParameterRecord> GetParameters(GetParametersRequest request)
    {
        dynamic doc = _app.ActiveDocument
            ?? throw new InvalidOperationException("No active Inventor document.");

        dynamic parameters = doc.ComponentDefinition.Parameters;
        var rows = new List<ParameterRecord>();

        if (request.IncludeUserParameters)
        {
            foreach (dynamic p in parameters.UserParameters)
            {
                rows.Add(new ParameterRecord
                {
                    Name = p.Name,
                    Units = SafeString(() => p.Units),
                    Expression = SafeString(() => p.Expression),
                    Value = SafeString(() => p.Expression),
                    IsKey = SafeBool(() => p.IsKey),
                    Comment = SafeString(() => p.Comment),
                });
            }
        }

        if (request.IncludeModelParameters)
        {
            foreach (dynamic p in parameters.ModelParameters)
            {
                rows.Add(new ParameterRecord
                {
                    Name = p.Name,
                    Units = SafeString(() => p.Units),
                    Expression = SafeString(() => p.Expression),
                    Value = SafeString(() => p.Expression),
                    IsKey = SafeBool(() => p.IsKey),
                    Comment = SafeString(() => p.Comment),
                });
            }
        }

        return rows;
    }

    public object SetParameters(SetParametersRequest request)
    {
        dynamic doc = _app.ActiveDocument
            ?? throw new InvalidOperationException("No active Inventor document.");

        dynamic parameters = doc.ComponentDefinition.Parameters;
        var updated = 0;
        foreach (var row in request.Parameters)
        {
            if (string.IsNullOrWhiteSpace(row.Name))
            {
                continue;
            }

            dynamic? target = null;
            try
            {
                target = parameters.Item(row.Name);
            }
            catch
            {
                // Inventor throws when parameter does not exist.
            }

            if (target is null)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(row.Expression))
            {
                target.Expression = row.Expression;
            }
            else if (row.Value is not null)
            {
                target.Expression = row.Value.ToString();
            }

            if (!string.IsNullOrWhiteSpace(row.Comment))
            {
                target.Comment = row.Comment;
            }

            updated++;
        }

        if (!string.IsNullOrWhiteSpace(request.RunRuleAfterSet))
        {
            _ = RunRule(request.RunRuleAfterSet, "activeDocument");
        }

        doc.Update2(true);
        return new
        {
            success = true,
            updated,
            documentDirty = doc.Dirty,
        };
    }

    public object ListDrawingViews()
    {
        var drawDoc = _app.ActiveDocument as DrawingDocument
            ?? throw new InvalidOperationException("Active document is not a drawing.");
        var sheet = drawDoc.ActiveSheet;
        var rows = new List<object>();
        for (var i = 1; i <= sheet.DrawingViews.Count; i++)
        {
            var view = sheet.DrawingViews[i];
            rows.Add(new
            {
                viewIndex = i,
                viewName = view.Name,
                left = view.Left,
                top = view.Top,
                width = view.Width,
                height = view.Height,
                scale = view.Scale,
            });
        }

        return new
        {
            drawing = drawDoc.DisplayName,
            sheet = sheet.Name,
            viewCount = rows.Count,
            views = rows,
        };
    }

    public object AddViewOverallDimensions(AddDrawingViewOverallDimensionsRequest request)
    {
        var drawDoc = _app.ActiveDocument as DrawingDocument
            ?? throw new InvalidOperationException("Active document is not a drawing.");
        var sheet = drawDoc.ActiveSheet;
        var view = ResolveDrawingView(sheet, request.ViewIndex, request.ViewName);
        var result = AddOverallDimensionsToView(drawDoc, sheet, view, request.OffsetInSheet, request.AddWidth, request.AddHeight);
        return new
        {
            drawing = drawDoc.DisplayName,
            sheet = sheet.Name,
            viewName = view.Name,
            viewIndex = result.ViewIndex,
            widthAdded = result.WidthAdded,
            heightAdded = result.HeightAdded,
            message = result.Message,
        };
    }

    public object AddAllViewsOverallDimensions(AddAllDrawingViewsOverallDimensionsRequest request)
    {
        var drawDoc = _app.ActiveDocument as DrawingDocument
            ?? throw new InvalidOperationException("Active document is not a drawing.");
        var sheet = drawDoc.ActiveSheet;
        var rows = new List<object>();
        var totalWidth = 0;
        var totalHeight = 0;

        for (var i = 1; i <= sheet.DrawingViews.Count; i++)
        {
            var view = sheet.DrawingViews[i];
            var result = AddOverallDimensionsToView(drawDoc, sheet, view, request.OffsetInSheet, request.AddWidth, request.AddHeight);
            if (result.WidthAdded)
            {
                totalWidth++;
            }
            if (result.HeightAdded)
            {
                totalHeight++;
            }

            rows.Add(new
            {
                viewIndex = result.ViewIndex,
                viewName = view.Name,
                widthAdded = result.WidthAdded,
                heightAdded = result.HeightAdded,
                message = result.Message,
            });
        }

        return new
        {
            drawing = drawDoc.DisplayName,
            sheet = sheet.Name,
            viewCount = sheet.DrawingViews.Count,
            widthDimsAdded = totalWidth,
            heightDimsAdded = totalHeight,
            results = rows,
        };
    }

    public object AddDiameterDimension(AddDiameterDimensionRequest request)
    {
        var drawDoc = _app.ActiveDocument as DrawingDocument
            ?? throw new InvalidOperationException("Active document is not a drawing.");
        var sheet = drawDoc.ActiveSheet;
        var view = ResolveDrawingView(sheet, request.ViewIndex, request.ViewName);

        var circles = new List<DrawingCurve>();
        foreach (DrawingCurve dc in view.DrawingCurves)
        {
            if (dc.CurveType.ToString().ToLower().Contains("circle"))
            {
                circles.Add(dc);
            }
        }

        if (request.CircleIndex < 0 || request.CircleIndex >= circles.Count)
        {
            throw new InvalidOperationException($"Circle index {request.CircleIndex} out of range (0..{circles.Count - 1}).");
        }

        var targetCircle = circles[request.CircleIndex];
        var intent = sheet.CreateGeometryIntent(targetCircle);

        double textX = view.Left + view.Width + 1.5;
        double textY = view.Top - (view.Height * 0.5);

        if (request.TextOffsetX.HasValue) textX = view.Left + request.TextOffsetX.Value;
        if (request.TextOffsetY.HasValue) textY = view.Top + request.TextOffsetY.Value;

        var textPoint = _app.TransientGeometry.CreatePoint2d(textX, textY);

        sheet.DrawingDimensions.GeneralDimensions.AddDiameter(textPoint, intent);

        drawDoc.Update();
        return new
        {
            drawing = drawDoc.DisplayName,
            viewName = view.Name,
            circleIndex = request.CircleIndex,
            success = true
        };
    }

    public object AddLinearDimension(AddLinearDimensionRequest request)
    {
        var drawDoc = _app.ActiveDocument as DrawingDocument
            ?? throw new InvalidOperationException("Active document is not a drawing.");
        var sheet = drawDoc.ActiveSheet;
        var view = ResolveDrawingView(sheet, request.ViewIndex, request.ViewName);

        DrawingCurve? fromCurve = null;
        DrawingCurve? toCurve = null;
        PointIntentEnum fromIntentType = PointIntentEnum.kMidPointIntent;
        PointIntentEnum toIntentType = PointIntentEnum.kMidPointIntent;

        // Collect circles and sort by center Y descending (top of view first) so indices are spatially ordered
        var circles = new List<DrawingCurve>();
        foreach (DrawingCurve dc in view.DrawingCurves)
        {
            if (dc.CurveType.ToString().ToLower().Contains("circle"))
            {
                circles.Add(dc);
            }
        }

        // Sort top-to-bottom using segment midpoint Y as proxy for center
        circles.Sort((a, b) =>
        {
            double ya = GetCurveCenterY(a);
            double yb = GetCurveCenterY(b);
            return yb.CompareTo(ya); // descending Y => top first
        });

        if (request.FromCircleIndex.HasValue)
        {
            int idx = request.FromCircleIndex.Value;
            if (idx < 0 || idx >= circles.Count)
            {
                throw new InvalidOperationException($"FromCircleIndex {idx} out of range (0..{circles.Count - 1}).");
            }
            fromCurve = circles[idx];
            fromIntentType = PointIntentEnum.kCenterPointIntent;
        }

        if (request.ToCircleIndex.HasValue)
        {
            int idx = request.ToCircleIndex.Value;
            if (idx < 0 || idx >= circles.Count)
            {
                throw new InvalidOperationException($"ToCircleIndex {idx} out of range (0..{circles.Count - 1}).");
            }
            toCurve = circles[idx];
            toIntentType = PointIntentEnum.kCenterPointIntent;
        }

        // Fallback: proximity search on lines (and circles if no explicit index) using midpoints/centers
        if (fromCurve == null || toCurve == null)
        {
            double bestFromDist = double.MaxValue;
            double bestToDist = double.MaxValue;

            foreach (DrawingCurve dc in view.DrawingCurves)
            {
                bool isCircle = dc.CurveType.ToString().ToLower().Contains("circle");
                bool isLine = dc.CurveType.ToString().ToLower().Contains("line");

                if (!isCircle && !isLine) continue;
                if (!isCircle && dc.Segments.Count == 0) continue;

                double mx, my;
                PointIntentEnum intentType;

                if (isCircle)
                {
                    // Approximate center from geometry bounds or segment
                    // For precision we rely on kCenterPointIntent later; use a representative point here for distance
                    var seg = dc.Segments[1];
                    // Rough center estimate (start is often near center for some curves, but better to use geometry if available)
                    // Fallback midpoint of first segment for distance calc only
                    mx = (seg.StartPoint.X + seg.EndPoint.X) / 2.0;
                    my = (seg.StartPoint.Y + seg.EndPoint.Y) / 2.0;
                    intentType = PointIntentEnum.kCenterPointIntent;
                }
                else
                {
                    var seg = dc.Segments[1];
                    mx = (seg.StartPoint.X + seg.EndPoint.X) / 2.0;
                    my = (seg.StartPoint.Y + seg.EndPoint.Y) / 2.0;
                    intentType = PointIntentEnum.kMidPointIntent;
                }

                if (fromCurve == null)
                {
                    double distFrom = Math.Sqrt(Math.Pow(mx - request.FromX, 2) + Math.Pow(my - request.FromY, 2));
                    if (distFrom < bestFromDist)
                    {
                        bestFromDist = distFrom;
                        fromCurve = dc;
                        fromIntentType = intentType;
                    }
                }

                if (toCurve == null)
                {
                    double distTo = Math.Sqrt(Math.Pow(mx - request.ToX, 2) + Math.Pow(my - request.ToY, 2));
                    if (distTo < bestToDist)
                    {
                        bestToDist = distTo;
                        toCurve = dc;
                        toIntentType = intentType;
                    }
                }
            }
        }

        if (fromCurve == null || toCurve == null)
        {
            throw new InvalidOperationException("Could not find suitable geometry (lines or circles) near the requested points in the view.");
        }

        var fromIntent = sheet.CreateGeometryIntent(fromCurve, fromIntentType);
        var toIntent = sheet.CreateGeometryIntent(toCurve, toIntentType);

        DimensionTypeEnum dimType = DimensionTypeEnum.kAlignedDimensionType;
        string dt = request.DimensionType?.ToLowerInvariant() ?? "aligned";
        if (dt == "horizontal") dimType = DimensionTypeEnum.kHorizontalDimensionType;
        else if (dt == "vertical") dimType = DimensionTypeEnum.kVerticalDimensionType;

        // Default text placement: if using circle indices, put the string dimension to the LEFT of the view
        double textX;
        double textY;

        if (request.FromCircleIndex.HasValue || request.ToCircleIndex.HasValue)
        {
            // Place to the left of the view, vertically centered between the two points
            double leftEdge = view.Left;
            textX = leftEdge - 1.5; // default offset to the left
            textY = (GetCurveCenterY(fromCurve!) + GetCurveCenterY(toCurve!)) / 2.0;
        }
        else
        {
            textX = (request.FromX + request.ToX) / 2.0 + 0.4;
            textY = (request.FromY + request.ToY) / 2.0 + 0.4;
        }

        if (request.TextOffsetX.HasValue)
        {
            textX = view.Left + request.TextOffsetX.Value;
        }
        if (request.TextOffsetY.HasValue)
        {
            textY = view.Top + request.TextOffsetY.Value;
        }

        var textPoint = _app.TransientGeometry.CreatePoint2d(textX, textY);

        sheet.DrawingDimensions.GeneralDimensions.AddLinear(textPoint, fromIntent, toIntent, dimType);

        drawDoc.Update();
        return new
        {
            drawing = drawDoc.DisplayName,
            viewName = view.Name,
            success = true
        };
    }

    public DocumentSettingsRecord GetDocumentSettings()
    {
        dynamic doc = _app.ActiveDocument
            ?? throw new InvalidOperationException("No active document.");

        dynamic? settings = null;
        try
        {
            settings = doc.DocumentSettings;
        }
        catch
        {
            // Drawing documents may not expose DocumentSettings; fall back to other APIs.
        }

        string? lengthUnits = null;
        if (settings is not null)
        {
            lengthUnits = SafeString(() => settings.LengthUnits.ToString());
        }
        if (string.IsNullOrWhiteSpace(lengthUnits))
        {
            lengthUnits = SafeString(() => doc.UnitsOfMeasure.LengthUnits.ToString());
        }

        return new DocumentSettingsRecord
        {
            LengthUnits = lengthUnits,
            AngleUnits = settings is not null ? SafeString(() => settings.AngleUnits.ToString()) : null,
            MassUnits = settings is not null ? SafeString(() => settings.MassUnits.ToString()) : null,
            LinearPrecision = settings is not null ? SafeInt(() => settings.LinearDimensionPrecision) : null,
            AngularPrecision = settings is not null ? SafeInt(() => settings.AngularDimensionPrecision) : null,
            ModelingDisplay = settings is not null ? SafeString(() => settings.ModelingDimensionDisplay.ToString()) : null,
            ShowSketches = settings is not null ? SafeBool(() => settings.ShowSketches) : null,
            ShowWorkFeatures = settings is not null ? SafeBool(() => settings.ShowWorkFeatures) : null,
        };
    }

    public object SetLengthUnits(SetLengthUnitsRequest request)
    {
        dynamic doc = _app.ActiveDocument
            ?? throw new InvalidOperationException("No active document.");

        var resolvedUnit = ResolveLengthUnit(request.Units);
        var applied = false;

        try
        {
            dynamic settings = doc.DocumentSettings;
            settings.LengthUnits = resolvedUnit;
            applied = true;
        }
        catch
        {
            // Expected for drawing docs on some Inventor COM paths.
        }

        try
        {
            doc.UnitsOfMeasure.LengthUnits = resolvedUnit;
            applied = true;
        }
        catch
        {
            // Not all docs expose UnitsOfMeasure in the same way.
        }

        var updatedStyles = ApplyLengthUnitsToDrawingStyles(doc, resolvedUnit);
        if (updatedStyles > 0)
        {
            applied = true;
        }

        if (!applied)
        {
            throw new InvalidOperationException("Could not apply length units on the active document.");
        }

        doc.Update2(true);
        return new { success = true, lengthUnits = request.Units, drawingStylesUpdated = updatedStyles };
    }

    public object SetAngleUnits(SetAngleUnitsRequest request)
    {
        dynamic doc = _app.ActiveDocument
            ?? throw new InvalidOperationException("No active document.");

        dynamic settings = doc.DocumentSettings;

        string unit = request.Units.ToLowerInvariant();
        switch (unit)
        {
            case "deg":
            case "degree":
            case "degrees":
                settings.AngleUnits = UnitsTypeEnum.kDegreeAngleUnits;
                break;
            case "rad":
            case "radian":
            case "radians":
                settings.AngleUnits = UnitsTypeEnum.kRadianAngleUnits;
                break;
            default:
                throw new InvalidOperationException($"Unsupported angle unit: {request.Units}");
        }

        doc.Update2(true);
        return new { success = true, angleUnits = request.Units };
    }

    public object SetMassUnits(SetMassUnitsRequest request)
    {
        dynamic doc = _app.ActiveDocument
            ?? throw new InvalidOperationException("No active document.");

        dynamic settings = doc.DocumentSettings;

        string unit = request.Units.ToLowerInvariant();
        switch (unit)
        {
            case "kg":
                settings.MassUnits = UnitsTypeEnum.kKilogramMassUnits;
                break;
            case "lb":
            case "pound":
                settings.MassUnits = UnitsTypeEnum.kSlugMassUnits; // fallback
                break;
            case "g":
            case "gram":
                settings.MassUnits = UnitsTypeEnum.kGramMassUnits;
                break;
            case "slug":
                settings.MassUnits = UnitsTypeEnum.kSlugMassUnits;
                break;
            default:
                throw new InvalidOperationException($"Unsupported mass unit: {request.Units}");
        }

        doc.Update2(true);
        return new { success = true, massUnits = request.Units };
    }

    public object SetDimensionPrecision(SetDimensionPrecisionRequest request)
    {
        dynamic doc = _app.ActiveDocument
            ?? throw new InvalidOperationException("No active document.");

        var applied = false;
        try
        {
            dynamic settings = doc.DocumentSettings;
            if (request.LinearPrecision.HasValue)
            {
                settings.LinearDimensionPrecision = request.LinearPrecision.Value;
                applied = true;
            }

            if (request.AngularPrecision.HasValue)
            {
                settings.AngularDimensionPrecision = request.AngularPrecision.Value;
                applied = true;
            }
        }
        catch
        {
            // Drawing docs may require style-level precision updates.
        }

        var updatedStyles = ApplyPrecisionToDrawingStyles(
            doc,
            request.LinearPrecision,
            request.AngularPrecision);
        if (updatedStyles > 0)
        {
            applied = true;
        }

        if (!applied)
        {
            throw new InvalidOperationException("Could not apply dimension precision on the active document.");
        }

        doc.Update2(true);
        return new
        {
            success = true,
            linearPrecision = request.LinearPrecision,
            angularPrecision = request.AngularPrecision,
            drawingStylesUpdated = updatedStyles
        };
    }

    private static UnitsTypeEnum ResolveLengthUnit(string units)
    {
        string unit = units.Trim().ToLowerInvariant();
        return unit switch
        {
            "in" or "inch" or "inches" => UnitsTypeEnum.kInchLengthUnits,
            "mm" => UnitsTypeEnum.kMillimeterLengthUnits,
            "cm" => UnitsTypeEnum.kCentimeterLengthUnits,
            "m" or "meter" => UnitsTypeEnum.kMeterLengthUnits,
            "ft" or "foot" or "feet" => UnitsTypeEnum.kFootLengthUnits,
            "architectural" or "arch" or "ft-in" or "feet-inches" =>
                TryParseUnitsType("kFeetAndInchesLengthUnits", UnitsTypeEnum.kFootLengthUnits),
            _ => throw new InvalidOperationException($"Unsupported length unit: {units}")
        };
    }

    private static UnitsTypeEnum TryParseUnitsType(string enumName, UnitsTypeEnum fallback)
    {
        try
        {
            var parsed = Enum.Parse(typeof(UnitsTypeEnum), enumName, ignoreCase: true);
            if (parsed is UnitsTypeEnum units)
            {
                return units;
            }
        }
        catch
        {
            // Fallback below.
        }

        return fallback;
    }

    private static int ApplyLengthUnitsToDrawingStyles(dynamic doc, UnitsTypeEnum resolvedUnit)
    {
        int updated = 0;
        try
        {
            dynamic styles = doc.StylesManager.DimensionStyles;
            int count = (int)styles.Count;
            for (int i = 1; i <= count; i++)
            {
                dynamic ds = styles.Item(i);
                bool changed = false;
                changed |= TrySetMember(ds, "LinearUnits", resolvedUnit);
                changed |= TrySetMember(ds, "PrimaryLinearUnits", resolvedUnit);
                if (changed)
                {
                    updated++;
                }
            }
        }
        catch
        {
            // Best effort.
        }

        return updated;
    }

    private static int ApplyPrecisionToDrawingStyles(dynamic doc, int? linearPrecision, int? angularPrecision)
    {
        int updated = 0;
        try
        {
            dynamic styles = doc.StylesManager.DimensionStyles;
            int count = (int)styles.Count;
            for (int i = 1; i <= count; i++)
            {
                dynamic ds = styles.Item(i);
                bool changed = false;
                if (linearPrecision.HasValue)
                {
                    // Prefer fractional enum values for drawing styles when possible.
                    var resolvedLinear = ResolveLinearPrecisionForDrawing(linearPrecision.Value);
                    if (resolvedLinear is not null)
                    {
                        changed |= TrySetMember(ds, "LinearPrecision", resolvedLinear);
                        changed |= TrySetMember(ds, "PrimaryLinearPrecision", resolvedLinear);
                    }

                    // Keep style units in feet+inches for architectural fractional workflows.
                    var feetInches = TryParseUnitsType("kFeetAndInchesLengthUnits", UnitsTypeEnum.kFootLengthUnits);
                    changed |= TrySetMember(ds, "LinearUnits", feetInches);
                    changed |= TrySetMember(ds, "PrimaryLinearUnits", feetInches);

                    // Fallback assignment for APIs that accept numeric precision directly.
                    changed |= TrySetMember(ds, "LinearPrecision", linearPrecision.Value);
                    changed |= TrySetMember(ds, "PrimaryLinearPrecision", linearPrecision.Value);
                }
                if (angularPrecision.HasValue)
                {
                    changed |= TrySetMember(ds, "AngularPrecision", angularPrecision.Value);
                    changed |= TrySetMember(ds, "PrimaryAngularPrecision", angularPrecision.Value);
                }

                if (changed)
                {
                    updated++;
                }
            }
        }
        catch
        {
            // Best effort.
        }

        return updated;
    }

    private static object? ResolveLinearPrecisionForDrawing(int precision)
    {
        string[]? candidateNames = precision switch
        {
            2 => new[] { "kHalvesFractionalLengthPrecision", "kHalfFractionalLengthPrecision" },
            4 => new[] { "kQuartersFractionalLengthPrecision", "kQuarterFractionalLengthPrecision" },
            8 => new[] { "kEighthsFractionalLengthPrecision", "kEighthFractionalLengthPrecision" },
            16 => new[] { "kSixteenthsFractionalLengthPrecision", "kSixteenthFractionalLengthPrecision" },
            32 => new[] { "kThirtySecondsFractionalLengthPrecision", "kThirtySecondFractionalLengthPrecision" },
            64 => new[] { "kSixtyFourthsFractionalLengthPrecision", "kSixtyFourthFractionalLengthPrecision" },
            _ => null
        };

        if (candidateNames is null)
        {
            return null;
        }

        foreach (var name in candidateNames)
        {
            try
            {
                var parsed = Enum.Parse(typeof(LinearPrecisionEnum), name, ignoreCase: true);
                if (parsed is LinearPrecisionEnum enumValue)
                {
                    return enumValue;
                }
            }
            catch
            {
                // Try next candidate.
            }
        }

        return null;
    }

    private static bool TrySetMember(dynamic target, string propertyName, object value)
    {
        try
        {
            target.GetType().InvokeMember(
                propertyName,
                System.Reflection.BindingFlags.SetProperty,
                binder: null,
                target: target,
                args: new object[] { value });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public object SetModelingDisplay(SetModelingDisplayRequest request)
    {
        dynamic doc = _app.ActiveDocument
            ?? throw new InvalidOperationException("No active document.");

        dynamic settings = doc.DocumentSettings;

        if (!string.IsNullOrWhiteSpace(request.DisplayMode))
        {
            // Modeling display mode setting simplified for initial release
            // Full enum support can be added after verifying correct Inventor enum name
        }

        if (request.ShowSketches.HasValue)
        {
            settings.ShowSketches = request.ShowSketches.Value;
        }

        if (request.ShowWorkFeatures.HasValue)
        {
            settings.ShowWorkFeatures = request.ShowWorkFeatures.Value;
        }

        doc.Update2(true);
        return new { success = true };
    }

    private dynamic? TryGetILogicAutomation()
    {
        try
        {
            const string iLogicClientId = "{3BDD8D79-2179-4B11-8A5A-257B1C0263AC}";
            dynamic addIns = _app.ApplicationAddIns;
            dynamic addIn = addIns.ItemById[iLogicClientId];
            if (addIn is null)
            {
                return null;
            }

            if (!addIn.Activated)
            {
                addIn.Activate();
            }

            return addIn.Automation;
        }
        catch
        {
            return null;
        }
    }

    private static bool TryInvokeAutomation(
        dynamic automation,
        string methodName,
        out string? methodUsed,
        params object?[] args)
    {
        methodUsed = null;
        try
        {
            var type = ((object)automation).GetType();
            var method = type.GetMethod(methodName, args.Select(a => a?.GetType() ?? typeof(object)).ToArray());
            if (method is not null)
            {
                method.Invoke(automation, args);
                methodUsed = methodName;
                return true;
            }
        }
        catch
        {
            // fall through to dynamic invoke attempts
        }

        try
        {
            if (args.Length == 2)
            {
                automation.GetType().InvokeMember(
                    methodName,
                    System.Reflection.BindingFlags.InvokeMethod,
                    null,
                    automation,
                    new object?[] { args[0], args[1] });
                methodUsed = methodName;
                return true;
            }
            if (args.Length == 1)
            {
                automation.GetType().InvokeMember(
                    methodName,
                    System.Reflection.BindingFlags.InvokeMethod,
                    null,
                    automation,
                    new object?[] { args[0] });
                methodUsed = methodName;
                return true;
            }
        }
        catch
        {
            // ignored - caller decides fallback method candidates
        }

        return false;
    }

    private static string? SafeString(Func<object?> getter)
    {
        try
        {
            return getter()?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static bool? SafeBool(Func<bool> getter)
    {
        try
        {
            return getter();
        }
        catch
        {
            return null;
        }
    }

    private static int? SafeInt(Func<int> getter)
    {
        try
        {
            return getter();
        }
        catch
        {
            return null;
        }
    }

    private static DrawingView ResolveDrawingView(Sheet sheet, int? viewIndex, string? viewName)
    {
        if (viewIndex.HasValue)
        {
            var idx = viewIndex.Value;
            if (idx < 1 || idx > sheet.DrawingViews.Count)
            {
                throw new InvalidOperationException($"viewIndex {idx} is out of range (1..{sheet.DrawingViews.Count}).");
            }

            return sheet.DrawingViews[idx];
        }

        if (!string.IsNullOrWhiteSpace(viewName))
        {
            var target = viewName.Trim();
            for (var i = 1; i <= sheet.DrawingViews.Count; i++)
            {
                var view = sheet.DrawingViews[i];
                if (string.Equals(view.Name, target, StringComparison.OrdinalIgnoreCase))
                {
                    return view;
                }
            }

            throw new InvalidOperationException($"Drawing view '{target}' was not found.");
        }

        throw new InvalidOperationException("Either viewIndex or viewName is required.");
    }

    private static (int ViewIndex, bool WidthAdded, bool HeightAdded, string Message) AddOverallDimensionsToView(
        DrawingDocument drawDoc,
        Sheet sheet,
        DrawingView view,
        double offsetInSheet,
        bool addWidth,
        bool addHeight)
    {
        if (offsetInSheet <= 0)
        {
            offsetInSheet = 1.0;
        }

        var app = (Inventor.Application)drawDoc.Parent;
        var tg = app.TransientGeometry;
        var viewIndex = FindViewIndex(sheet, view);
        if (!TryFindBoundaryCurves(view, out var leftCurve, out var rightCurve, out var bottomCurve, out var topCurve))
        {
            return (viewIndex, false, false, "No rectangular boundary linework found for this view.");
        }

        var minX = view.Left;
        var maxX = view.Left + view.Width;
        var maxY = view.Top;
        var minY = view.Top - view.Height;

        var widthAdded = false;
        var heightAdded = false;

        if (addWidth)
        {
            try
            {
                var leftIntent = sheet.CreateGeometryIntent(leftCurve!, PointIntentEnum.kMidPointIntent);
                var rightIntent = sheet.CreateGeometryIntent(rightCurve!, PointIntentEnum.kMidPointIntent);
                var textPoint = tg.CreatePoint2d((minX + maxX) * 0.5, maxY + offsetInSheet);
                _ = sheet.DrawingDimensions.GeneralDimensions.AddLinear(
                    textPoint,
                    leftIntent,
                    rightIntent,
                    DimensionTypeEnum.kHorizontalDimensionType);
                widthAdded = true;
            }
            catch
            {
                // Best effort per view; callers receive status.
            }
        }

        if (addHeight)
        {
            try
            {
                var bottomIntent = sheet.CreateGeometryIntent(bottomCurve!, PointIntentEnum.kMidPointIntent);
                var topIntent = sheet.CreateGeometryIntent(topCurve!, PointIntentEnum.kMidPointIntent);
                var textPoint = tg.CreatePoint2d(maxX + offsetInSheet, (minY + maxY) * 0.5);
                _ = sheet.DrawingDimensions.GeneralDimensions.AddLinear(
                    textPoint,
                    bottomIntent,
                    topIntent,
                    DimensionTypeEnum.kVerticalDimensionType);
                heightAdded = true;
            }
            catch
            {
                // Best effort per view; callers receive status.
            }
        }

        drawDoc.Update();
        return (viewIndex, widthAdded, heightAdded, "ok");
    }

    private static int FindViewIndex(Sheet sheet, DrawingView target)
    {
        for (var i = 1; i <= sheet.DrawingViews.Count; i++)
        {
            if (string.Equals(sheet.DrawingViews[i].Name, target.Name, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    private static bool TryFindBoundaryCurves(
        DrawingView view,
        out DrawingCurve? leftCurve,
        out DrawingCurve? rightCurve,
        out DrawingCurve? bottomCurve,
        out DrawingCurve? topCurve)
    {
        leftCurve = null;
        rightCurve = null;
        bottomCurve = null;
        topCurve = null;

        var leftX = double.MaxValue;
        var rightX = double.MinValue;
        var bottomY = double.MaxValue;
        var topY = double.MinValue;

        foreach (DrawingCurve curve in view.DrawingCurves)
        {
            if (!curve.CurveType.ToString().Contains("LineSegment", StringComparison.OrdinalIgnoreCase) || curve.Segments.Count < 1)
            {
                continue;
            }

            var segment = curve.Segments[1];
            var p1 = segment.StartPoint;
            var p2 = segment.EndPoint;
            var dx = Math.Abs(p2.X - p1.X);
            var dy = Math.Abs(p2.Y - p1.Y);
            var midX = (p1.X + p2.X) * 0.5;
            var midY = (p1.Y + p2.Y) * 0.5;

            if (dx >= dy)
            {
                if (midY < bottomY)
                {
                    bottomY = midY;
                    bottomCurve = curve;
                }

                if (midY > topY)
                {
                    topY = midY;
                    topCurve = curve;
                }
            }
            else
            {
                if (midX < leftX)
                {
                    leftX = midX;
                    leftCurve = curve;
                }

                if (midX > rightX)
                {
                    rightX = midX;
                    rightCurve = curve;
                }
            }
        }

        return leftCurve is not null && rightCurve is not null && bottomCurve is not null && topCurve is not null;
    }

    private static double GetCurveCenterY(DrawingCurve curve)
    {
        if (curve.Segments.Count == 0)
        {
            return 0;
        }
        var seg = curve.Segments[1];
        return (seg.StartPoint.Y + seg.EndPoint.Y) / 2.0;
    }
}
