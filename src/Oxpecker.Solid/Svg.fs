namespace Oxpecker.Solid

open Fable.Core
module Svg =

    /// Shared SVG attribute group for conditional processing elements.
    [<AllowNullLiteral>]
    type ConditionalProcessingSVGAttributes = interface end

    /// Shared attributes for SVG animation elements.
    [<AllowNullLiteral>]
    type AnimationElementSVGAttributes =
        inherit ConditionalProcessingSVGAttributes

    /// Shared attributes for SVG geometric shapes.
    [<AllowNullLiteral>]
    type ShapeElementSVGAttributes = interface end

    /// Shared attributes for SVG containers with graphical children.
    [<AllowNullLiteral>]
    type ContainerElementSVGAttributes =
        inherit ShapeElementSVGAttributes

    /// Shared attributes for SVG filter primitives.
    [<AllowNullLiteral>]
    type FilterPrimitiveElementSVGAttributes = interface end

    /// Shared SVG attribute group for transformable elements.
    [<AllowNullLiteral>]
    type TransformableSVGAttributes = interface end

    /// Shared SVG attribute group for animation timing elements.
    [<AllowNullLiteral>]
    type AnimationTimingSVGAttributes = interface end

    /// Shared SVG attribute group for animation value elements.
    [<AllowNullLiteral>]
    type AnimationValueSVGAttributes = interface end

    /// Shared SVG attribute group for animation addition elements.
    [<AllowNullLiteral>]
    type AnimationAdditionSVGAttributes = interface end

    /// Shared SVG attribute group for animation attribute target elements.
    [<AllowNullLiteral>]
    type AnimationAttributeTargetSVGAttributes = interface end

    /// Shared SVG attribute group for presentation elements.
    [<AllowNullLiteral>]
    type PresentationSVGAttributes = interface end

    /// Shared SVG attribute group for single input filter elements.
    [<AllowNullLiteral>]
    type SingleInputFilterSVGAttributes = interface end

    /// Shared SVG attribute group for double input filter elements.
    [<AllowNullLiteral>]
    type DoubleInputFilterSVGAttributes = interface end

    /// Shared SVG attribute group for fit to view box elements.
    [<AllowNullLiteral>]
    type FitToViewBoxSVGAttributes = interface end

    /// Shared attributes for SVG gradients.
    [<AllowNullLiteral>]
    type GradientElementSVGAttributes = interface end

    /// Shared attributes for SVG graphics elements.
    [<AllowNullLiteral>]
    type GraphicsElementSVGAttributes = interface end

    /// Shared attributes for SVG light-source elements.
    [<AllowNullLiteral>]
    type LightSourceElementSVGAttributes = interface end

    /// Shared SVG attribute group for new viewport elements.
    [<AllowNullLiteral>]
    type NewViewportSVGAttributes = interface end

    /// Shared font, paint, and layout attributes for SVG text content.
    [<AllowNullLiteral>]
    type TextContentElementSVGAttributes = interface end

    /// Shared SVG attribute group for zoom and pan elements.
    [<AllowNullLiteral>]
    type ZoomAndPanSVGAttributes = interface end

    type TransformableSVGAttributes with
        /// Applies SVG transformations such as `translate`, `rotate`, `scale`, or `matrix`.
        [<Erase>]
        member _.transform
            with set (_: string) = ()

    type ConditionalProcessingSVGAttributes with
        /// Space-separated extension identifiers required for this element to be processed.
        [<Erase>]
        member _.requiredExtensions
            with set (_: string) = ()
        /// Legacy feature test; prefer feature detection in script.
        [<Erase>]
        member _.requiredFeatures
            with set (_: string) = ()
        /// Language tags for which this SVG element is intended.
        [<Erase>]
        member _.systemLanguage
            with set (_: string) = ()

    type AnimationTimingSVGAttributes with
        /// Animation start time or event condition.
        [<Erase>]
        member _.begin'
            with set (_: string) = ()
        /// Animation duration, such as `2s`, `500ms`, or `indefinite`.
        [<Erase>]
        member _.dur
            with set (_: string) = ()
        /// Animation end time or event condition.
        [<Erase>]
        member _.end'
            with set (_: string) = ()
        /// Minimum active duration of an animation.
        [<Erase>]
        member _.min
            with set (_: string) = ()
        /// Maximum active duration of an animation.
        [<Erase>]
        member _.max
            with set (_: string) = ()
        /// Animation restart policy: `always`, `whenNotActive`, or `never`.
        [<Erase>]
        member _.restart
            with set (_: string) = ()
        /// Number of animation repetitions or `indefinite`.
        [<Erase>]
        member _.repeatCount
            with set (_: string) = ()
        /// Total repeat duration or `indefinite`.
        [<Erase>]
        member _.repeatDur
            with set (_: string) = ()
        /// For graphics sets the fill paint; for animation timing, controls whether the animated value is retained (`freeze`) or removed after completion (`remove`).
        [<Erase>]
        member _.fill
            with set (_: string) = ()

    type AnimationValueSVGAttributes with
        /// Animation interpolation mode: `discrete`, `linear`, `paced`, or `spline`.
        [<Erase>]
        member _.calcMode
            with set (_: string) = ()
        /// Semicolon-separated values used by an animation or filter primitive.
        [<Erase>]
        member _.values
            with set (_: string) = ()
        /// Semicolon-separated normalized key times corresponding to animation values.
        [<Erase>]
        member _.keyTimes
            with set (_: string) = ()
        /// Semicolon-separated Bezier control points for spline animation segments.
        [<Erase>]
        member _.keySplines
            with set (_: string) = ()
        /// Starting value for an SVG animation.
        [<Erase>]
        member _.from
            with set (_: string) = ()
        /// Target value for an SVG animation.
        [<Erase>]
        member _.to'
            with set (_: string) = ()
        /// Relative change applied by an SVG animation.
        [<Erase>]
        member _.by
            with set (_: string) = ()

    type AnimationAdditionSVGAttributes with
        /// Name of the target attribute changed by an SVG animation.
        [<Erase>]
        member _.attributeName
            with set (_: string) = ()
        /// Whether an animation adds to the underlying value: `replace` or `sum`.
        [<Erase>]
        member _.additive
            with set (_: string) = ()
        /// Whether repeated iterations build on earlier values: `none` or `sum`.
        [<Erase>]
        member _.accumulate
            with set (_: string) = ()

    type AnimationAttributeTargetSVGAttributes with
        /// Name of the target attribute changed by an SVG animation.
        [<Erase>]
        member _.attributeName
            with set (_: string) = ()
        /// Animation target namespace: `auto`, `CSS`, or `XML`.
        [<Erase>]
        member _.attributeType
            with set (_: string) = ()

    type ContainerElementSVGAttributes with
        /// Rendering quality hint: `auto`, `optimizeSpeed`, or `optimizeQuality`.
        [<Erase>]
        member _.``color-rendering``
            with set (_: string) = ()
        /// URL reference to a clipping path, commonly `url(#clip-id)`.
        [<Erase>]
        member _.``clip-path``
            with set (_: string) = ()
        /// Cursor to display over the element, such as `pointer` or `move`.
        [<Erase>]
        member _.cursor
            with set (_: string) = ()
        /// Color interpolation space: `auto`, `sRGB`, or `linearRGB`.
        [<Erase>]
        member _.``color-interpolation``
            with set (_: string) = ()
        /// Legacy control for the background image available to filter effects.
        [<Erase>]
        member _.``enable-background``
            with set (_: string) = ()
        /// URL reference to an SVG filter, commonly `url(#filter-id)`.
        [<Erase>]
        member _.filter
            with set (_: string) = ()
        /// URL reference to an SVG mask, commonly `url(#mask-id)`.
        [<Erase>]
        member _.mask
            with set (_: string) = ()
        /// Overall opacity from `0` (transparent) to `1` (opaque).
        [<Erase>]
        member _.opacity
            with set (_: string) = ()

    type GraphicsElementSVGAttributes with
        /// Clipping-path fill rule: `nonzero` or `evenodd`.
        [<Erase>]
        member _.``clip-rule``
            with set (_: string) = ()
        /// URL reference to an SVG mask, commonly `url(#mask-id)`.
        [<Erase>]
        member _.mask
            with set (_: string) = ()
        /// Conditions for pointer targeting, such as `auto`, `none`, `visiblePainted`, or `all`.
        [<Erase>]
        member _.``pointer-events``
            with set (_: string) = ()
        /// Cursor to display over the element, such as `pointer` or `move`.
        [<Erase>]
        member _.cursor
            with set (_: string) = ()
        /// Overall opacity from `0` (transparent) to `1` (opaque).
        [<Erase>]
        member _.opacity
            with set (_: string) = ()
        /// URL reference to an SVG filter, commonly `url(#filter-id)`.
        [<Erase>]
        member _.filter
            with set (_: string) = ()
        /// Whether the element participates in rendering; `none` hides it.
        [<Erase>]
        member _.display
            with set (_: string) = ()
        /// Visibility state: `visible`, `hidden`, or `collapse`.
        [<Erase>]
        member _.visibility
            with set (_: string) = ()
        /// Color interpolation space: `auto`, `sRGB`, or `linearRGB`.
        [<Erase>]
        member _.``color-interpolation``
            with set (_: string) = ()
        /// Rendering quality hint: `auto`, `optimizeSpeed`, or `optimizeQuality`.
        [<Erase>]
        member _.``color-rendering``
            with set (_: string) = ()
        /// Paint used for an outline; accepts a color, `none`, or paint-server URL.
        [<Erase>]
        member _.stroke
            with set (_: string) = ()
        /// Lengths defining a repeating dashed stroke pattern; `none` gives a solid line.
        [<Erase>]
        member _.``stroke-dasharray``
            with set (_: string) = ()
        /// Offset into the stroke dash pattern.
        [<Erase>]
        member _.``stroke-dashoffset``
            with set (_: string) = ()
        /// Stroke-end shape: `butt`, `round`, or `square`.
        [<Erase>]
        member _.``stroke-linecap``
            with set (_: string) = ()
        /// Stroke-join shape: `arcs`, `bevel`, `miter`, `miter-clip`, or `round`.
        [<Erase>]
        member _.``stroke-linejoin``
            with set (_: string) = ()
        /// Maximum miter length relative to stroke width before the join is beveled.
        [<Erase>]
        member _.``stroke-miterlimit``
            with set (_: string) = ()
        /// Stroke opacity from `0` to `1`.
        [<Erase>]
        member _.``stroke-opacity``
            with set (_: string) = ()
        /// Width of the element outline.
        [<Erase>]
        member _.``stroke-width``
            with set (_: string) = ()
        /// Current color used by SVG paint properties such as `fill` and `stroke`.
        [<Erase>]
        member _.color
            with set (_: string) = ()
        /// For graphics sets the fill paint; for animation timing, controls whether the animated value is retained (`freeze`) or removed after completion (`remove`).
        [<Erase>]
        member _.fill
            with set (_: string) = ()
        /// Fill opacity from `0` to `1`.
        [<Erase>]
        member _.``fill-opacity``
            with set (_: string) = ()
        /// Path interior rule: `nonzero` or `evenodd`.
        [<Erase>]
        member _.``fill-rule``
            with set (_: string) = ()
        /// Rendering-quality hint: `auto`, `optimizeSpeed`, `crispEdges`, or `geometricPrecision`.
        [<Erase>]
        member _.``shape-rendering``
            with set (_: string) = ()
        /// Author-provided total path length used to calibrate distance calculations.
        [<Erase>]
        member _.pathLength
            with set (_: string) = ()

    type TextContentElementSVGAttributes with
        /// Font family list used to render text.
        [<Erase>]
        member _.``font-family``
            with set (_: string) = ()
        /// Text font size.
        [<Erase>]
        member _.``font-size``
            with set (_: string) = ()
        /// Adjusts font size to preserve a font metric such as x-height.
        [<Erase>]
        member _.``font-size-adjust``
            with set (_: string) = ()
        /// Font width variant, such as `condensed`, `normal`, or `expanded`.
        [<Erase>]
        member _.``font-stretch``
            with set (_: string) = ()
        /// Font style such as `normal`, `italic`, or `oblique`.
        [<Erase>]
        member _.``font-style``
            with set (_: string) = ()
        /// Font variant, for example `normal` or `small-caps`.
        [<Erase>]
        member _.``font-variant``
            with set (_: string) = ()
        /// Font weight, such as `normal`, `bold`, or a numeric weight.
        [<Erase>]
        member _.``font-weight``
            with set (_: string) = ()
        /// Legacy text kerning adjustment.
        [<Erase>]
        member _.kerning
            with set (_: string) = ()
        /// Additional spacing between text characters.
        [<Erase>]
        member _.``letter-spacing``
            with set (_: string) = ()
        /// Additional spacing between words.
        [<Erase>]
        member _.``word-spacing``
            with set (_: string) = ()
        /// Text decoration such as `underline`, `overline`, or `line-through`.
        [<Erase>]
        member _.``text-decoration``
            with set (_: string) = ()
        /// Legacy orientation adjustment for glyphs in horizontal text.
        [<Erase>]
        member _.``glyph-orientation-horizontal``
            with set (_: string) = ()
        /// Legacy orientation adjustment for glyphs in vertical text.
        [<Erase>]
        member _.``glyph-orientation-vertical``
            with set (_: string) = ()
        /// Text direction: `ltr` or `rtl`.
        [<Erase>]
        member _.direction
            with set (_: string) = ()
        /// How bidirectional text embedding and overrides are handled.
        [<Erase>]
        member _.``unicode-bidi``
            with set (_: string) = ()
        /// Text alignment relative to its position: `start`, `middle`, or `end`.
        [<Erase>]
        member _.``text-anchor``
            with set (_: string) = ()
        /// Baseline used to align text, such as `auto`, `middle`, `central`, or `hanging`.
        [<Erase>]
        member _.``dominant-baseline``
            with set (_: string) = ()
        /// Current color used by SVG paint properties such as `fill` and `stroke`.
        [<Erase>]
        member _.color
            with set (_: string) = ()
        /// For graphics sets the fill paint; for animation timing, controls whether the animated value is retained (`freeze`) or removed after completion (`remove`).
        [<Erase>]
        member _.fill
            with set (_: string) = ()
        /// Fill opacity from `0` to `1`.
        [<Erase>]
        member _.``fill-opacity``
            with set (_: string) = ()
        /// Path interior rule: `nonzero` or `evenodd`.
        [<Erase>]
        member _.``fill-rule``
            with set (_: string) = ()
        /// Paint used for an outline; accepts a color, `none`, or paint-server URL.
        [<Erase>]
        member _.stroke
            with set (_: string) = ()
        /// Lengths defining a repeating dashed stroke pattern; `none` gives a solid line.
        [<Erase>]
        member _.``stroke-dasharray``
            with set (_: string) = ()
        /// Offset into the stroke dash pattern.
        [<Erase>]
        member _.``stroke-dashoffset``
            with set (_: string) = ()
        /// Stroke-end shape: `butt`, `round`, or `square`.
        [<Erase>]
        member _.``stroke-linecap``
            with set (_: string) = ()
        /// Stroke-join shape: `arcs`, `bevel`, `miter`, `miter-clip`, or `round`.
        [<Erase>]
        member _.``stroke-linejoin``
            with set (_: string) = ()
        /// Maximum miter length relative to stroke width before the join is beveled.
        [<Erase>]
        member _.``stroke-miterlimit``
            with set (_: string) = ()
        /// Stroke opacity from `0` to `1`.
        [<Erase>]
        member _.``stroke-opacity``
            with set (_: string) = ()
        /// Width of the element outline.
        [<Erase>]
        member _.``stroke-width``
            with set (_: string) = ()

    type PresentationSVGAttributes with
        /// Baseline of this element aligned with its parent text run.
        [<Erase>]
        member _.``alignment-baseline``
            with set (_: string) = ()
        /// Shifts the dominant baseline by a length or percentage.
        [<Erase>]
        member _.``baseline-shift``
            with set (_: string) = ()
        /// Legacy clipping rectangle; prefer `clip-path`.
        [<Erase>]
        member _.clip
            with set (_: string) = ()
        /// URL reference to a clipping path, commonly `url(#clip-id)`.
        [<Erase>]
        member _.``clip-path``
            with set (_: string) = ()
        /// Clipping-path fill rule: `nonzero` or `evenodd`.
        [<Erase>]
        member _.``clip-rule``
            with set (_: string) = ()
        /// Current color used by SVG paint properties such as `fill` and `stroke`.
        [<Erase>]
        member _.color
            with set (_: string) = ()
        /// Color interpolation space: `auto`, `sRGB`, or `linearRGB`.
        [<Erase>]
        member _.``color-interpolation``
            with set (_: string) = ()
        /// Color space used for filter calculations: `auto`, `sRGB`, or `linearRGB`.
        [<Erase>]
        member _.``color-interpolation-filters``
            with set (_: string) = ()
        /// Legacy color profile hint for rendering an image.
        [<Erase>]
        member _.``color-profile``
            with set (_: string) = ()
        /// Rendering quality hint: `auto`, `optimizeSpeed`, or `optimizeQuality`.
        [<Erase>]
        member _.``color-rendering``
            with set (_: string) = ()
        /// Cursor to display over the element, such as `pointer` or `move`.
        [<Erase>]
        member _.cursor
            with set (_: string) = ()
        /// Text direction: `ltr` or `rtl`.
        [<Erase>]
        member _.direction
            with set (_: string) = ()
        /// Whether the element participates in rendering; `none` hides it.
        [<Erase>]
        member _.display
            with set (_: string) = ()
        /// Baseline used to align text, such as `auto`, `middle`, `central`, or `hanging`.
        [<Erase>]
        member _.``dominant-baseline``
            with set (_: string) = ()
        /// Legacy control for the background image available to filter effects.
        [<Erase>]
        member _.``enable-background``
            with set (_: string) = ()
        /// For graphics sets the fill paint; for animation timing, controls whether the animated value is retained (`freeze`) or removed after completion (`remove`).
        [<Erase>]
        member _.fill
            with set (_: string) = ()
        /// Fill opacity from `0` to `1`.
        [<Erase>]
        member _.``fill-opacity``
            with set (_: string) = ()
        /// Path interior rule: `nonzero` or `evenodd`.
        [<Erase>]
        member _.``fill-rule``
            with set (_: string) = ()
        /// URL reference to an SVG filter, commonly `url(#filter-id)`.
        [<Erase>]
        member _.filter
            with set (_: string) = ()
        /// Color produced by a flood filter primitive.
        [<Erase>]
        member _.``flood-color``
            with set (_: string) = ()
        /// Flood opacity from `0` to `1`.
        [<Erase>]
        member _.``flood-opacity``
            with set (_: string) = ()
        /// Font family list used to render text.
        [<Erase>]
        member _.``font-family``
            with set (_: string) = ()
        /// Text font size.
        [<Erase>]
        member _.``font-size``
            with set (_: string) = ()
        /// Adjusts font size to preserve a font metric such as x-height.
        [<Erase>]
        member _.``font-size-adjust``
            with set (_: string) = ()
        /// Font width variant, such as `condensed`, `normal`, or `expanded`.
        [<Erase>]
        member _.``font-stretch``
            with set (_: string) = ()
        /// Font style such as `normal`, `italic`, or `oblique`.
        [<Erase>]
        member _.``font-style``
            with set (_: string) = ()
        /// Font variant, for example `normal` or `small-caps`.
        [<Erase>]
        member _.``font-variant``
            with set (_: string) = ()
        /// Font weight, such as `normal`, `bold`, or a numeric weight.
        [<Erase>]
        member _.``font-weight``
            with set (_: string) = ()
        /// Legacy orientation adjustment for glyphs in horizontal text.
        [<Erase>]
        member _.``glyph-orientation-horizontal``
            with set (_: string) = ()
        /// Legacy orientation adjustment for glyphs in vertical text.
        [<Erase>]
        member _.``glyph-orientation-vertical``
            with set (_: string) = ()
        /// Image rendering hint: `auto`, `optimizeSpeed`, or `optimizeQuality`.
        [<Erase>]
        member _.``image-rendering``
            with set (_: string) = ()
        /// Legacy text kerning adjustment.
        [<Erase>]
        member _.kerning
            with set (_: string) = ()
        /// Additional spacing between text characters.
        [<Erase>]
        member _.``letter-spacing``
            with set (_: string) = ()
        /// Color of the light used by a lighting filter.
        [<Erase>]
        member _.``lighting-color``
            with set (_: string) = ()
        /// Marker placed at the final vertex of a path.
        [<Erase>]
        member _.``marker-end``
            with set (_: string) = ()
        /// Marker placed at interior vertices of a path.
        [<Erase>]
        member _.``marker-mid``
            with set (_: string) = ()
        /// Marker placed at the initial vertex of a path.
        [<Erase>]
        member _.``marker-start``
            with set (_: string) = ()
        /// URL reference to an SVG mask, commonly `url(#mask-id)`.
        [<Erase>]
        member _.mask
            with set (_: string) = ()
        /// Overall opacity from `0` (transparent) to `1` (opaque).
        [<Erase>]
        member _.opacity
            with set (_: string) = ()
        /// Handling of overflowing content: `visible`, `hidden`, `scroll`, or `auto`.
        [<Erase>]
        member _.overflow
            with set (_: string) = ()
        /// Author-provided total path length used to calibrate distance calculations.
        [<Erase>]
        member _.pathLength
            with set (_: string) = ()
        /// Conditions for pointer targeting, such as `auto`, `none`, `visiblePainted`, or `all`.
        [<Erase>]
        member _.``pointer-events``
            with set (_: string) = ()
        /// Rendering-quality hint: `auto`, `optimizeSpeed`, `crispEdges`, or `geometricPrecision`.
        [<Erase>]
        member _.``shape-rendering``
            with set (_: string) = ()
        /// Color at a gradient stop.
        [<Erase>]
        member _.``stop-color``
            with set (_: string) = ()
        /// Gradient stop opacity from `0` to `1`.
        [<Erase>]
        member _.``stop-opacity``
            with set (_: string) = ()
        /// Paint used for an outline; accepts a color, `none`, or paint-server URL.
        [<Erase>]
        member _.stroke
            with set (_: string) = ()
        /// Lengths defining a repeating dashed stroke pattern; `none` gives a solid line.
        [<Erase>]
        member _.``stroke-dasharray``
            with set (_: string) = ()
        /// Offset into the stroke dash pattern.
        [<Erase>]
        member _.``stroke-dashoffset``
            with set (_: string) = ()
        /// Stroke-end shape: `butt`, `round`, or `square`.
        [<Erase>]
        member _.``stroke-linecap``
            with set (_: string) = ()
        /// Stroke-join shape: `arcs`, `bevel`, `miter`, `miter-clip`, or `round`.
        [<Erase>]
        member _.``stroke-linejoin``
            with set (_: string) = ()
        /// Maximum miter length relative to stroke width before the join is beveled.
        [<Erase>]
        member _.``stroke-miterlimit``
            with set (_: string) = ()
        /// Stroke opacity from `0` to `1`.
        [<Erase>]
        member _.``stroke-opacity``
            with set (_: string) = ()
        /// Width of the element outline.
        [<Erase>]
        member _.``stroke-width``
            with set (_: string) = ()
        /// Text alignment relative to its position: `start`, `middle`, or `end`.
        [<Erase>]
        member _.``text-anchor``
            with set (_: string) = ()
        /// Text decoration such as `underline`, `overline`, or `line-through`.
        [<Erase>]
        member _.``text-decoration``
            with set (_: string) = ()
        /// Hint for balancing text rendering speed and legibility.
        [<Erase>]
        member _.``text-rendering``
            with set (_: string) = ()
        /// How bidirectional text embedding and overrides are handled.
        [<Erase>]
        member _.``unicode-bidi``
            with set (_: string) = ()
        /// Visibility state: `visible`, `hidden`, or `collapse`.
        [<Erase>]
        member _.visibility
            with set (_: string) = ()
        /// Additional spacing between words.
        [<Erase>]
        member _.``word-spacing``
            with set (_: string) = ()
        /// Text writing direction and line progression.
        [<Erase>]
        member _.``writing-mode``
            with set (_: string) = ()

    type FilterPrimitiveElementSVGAttributes with
        /// Horizontal position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.x
            with set (_: string) = ()
        /// Vertical position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.y
            with set (_: string) = ()
        /// Width of an SVG viewport, shape, filter region, or primitive.
        [<Erase>]
        member _.width
            with set (_: string) = ()
        /// Height of an SVG viewport, shape, filter region, or primitive.
        [<Erase>]
        member _.height
            with set (_: string) = ()
        /// Name assigned to this filter primitive output for later primitives.
        [<Erase>]
        member _.result
            with set (_: string) = ()
        /// Color space used for filter calculations: `auto`, `sRGB`, or `linearRGB`.
        [<Erase>]
        member _.``color-interpolation-filters``
            with set (_: string) = ()

    type SingleInputFilterSVGAttributes with
        /// Input graphic or previous filter result consumed by this primitive.
        [<Erase>]
        member _.in'
            with set (_: string) = ()

    type DoubleInputFilterSVGAttributes with
        /// Input graphic or previous filter result consumed by this primitive.
        [<Erase>]
        member _.in'
            with set (_: string) = ()
        /// Second input graphic or filter result consumed by this primitive.
        [<Erase>]
        member _.in2
            with set (_: string) = ()

    type FitToViewBoxSVGAttributes with
        /// Four numbers defining the SVG view rectangle: minimum x, minimum y, width, and height.
        [<Erase>]
        member _.viewBox
            with set (_: string) = ()
        /// How the viewBox is fitted into the viewport and whether its aspect ratio is preserved.
        [<Erase>]
        member _.preserveAspectRatio
            with set (_: string) = ()

    type GradientElementSVGAttributes with
        /// Gradient coordinate system: `objectBoundingBox` or `userSpaceOnUse`.
        [<Erase>]
        member _.gradientUnits
            with set (_: string) = ()
        /// Transformation applied to the gradient coordinate system.
        [<Erase>]
        member _.gradientTransform
            with set (_: string) = ()
        /// Gradient behavior outside endpoints: `pad`, `reflect`, or `repeat`.
        [<Erase>]
        member _.spreadMethod
            with set (_: string) = ()
        /// URL reference to another SVG resource or element.
        [<Erase>]
        member _.href
            with set (_: string) = ()

    /// Animates an attribute or property of an SVG element.
    [<Erase>]
    type animate() =
        interface RegularNode
        interface AnimationElementSVGAttributes
        interface AnimationAttributeTargetSVGAttributes
        interface AnimationTimingSVGAttributes
        interface AnimationValueSVGAttributes
        interface AnimationAdditionSVGAttributes
        /// Color interpolation space: `auto`, `sRGB`, or `linearRGB`.
        [<Erase>]
        member _.``color-interpolation``
            with set (_: string) = ()
        /// Rendering quality hint: `auto`, `optimizeSpeed`, or `optimizeQuality`.
        [<Erase>]
        member _.``color-rendering``
            with set (_: string) = ()

    /// Animates an element along a motion path.
    [<Erase>]
    type animateMotion() =
        interface RegularNode
        interface AnimationElementSVGAttributes
        interface AnimationTimingSVGAttributes
        interface AnimationValueSVGAttributes
        interface AnimationAdditionSVGAttributes
        /// SVG path data or motion path, depending on the animation element.
        [<Erase>]
        member _.path
            with set (_: string) = ()
        /// Semicolon-separated normalized points along a motion path.
        [<Erase>]
        member _.keyPoints
            with set (_: string) = ()
        /// Rotation angle or `auto`/`auto-reverse` for motion animation.
        [<Erase>]
        member _.rotate
            with set (_: string) = ()
        /// Origin used for motion animation.
        [<Erase>]
        member _.origin
            with set (_: string) = ()

    /// Animates an SVG transform such as translation, rotation, or scaling.
    [<Erase>]
    type animateTransform() =
        interface RegularNode
        interface AnimationElementSVGAttributes
        interface AnimationAttributeTargetSVGAttributes
        interface AnimationTimingSVGAttributes
        interface AnimationValueSVGAttributes
        interface AnimationAdditionSVGAttributes
        /// Selects an SVG operation or value interpretation; allowed values depend on the element.
        [<Erase>]
        member _.type'
            with set (_: string) = ()

    /// Draws a circle.
    [<Erase>]
    type circle() =
        interface RegularNode
        interface GraphicsElementSVGAttributes
        interface ShapeElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface TransformableSVGAttributes
        /// Horizontal center coordinate of a circle, ellipse, or radial gradient.
        [<Erase>]
        member _.cx
            with set (_: string) = ()
        /// Vertical center coordinate of a circle, ellipse, or radial gradient.
        [<Erase>]
        member _.cy
            with set (_: string) = ()
        /// Radius of a circle or radial gradient.
        [<Erase>]
        member _.r
            with set (_: string) = ()

    /// Defines a clipping path that determines which parts of graphics remain visible.
    [<Erase>]
    type clipPath() =
        interface RegularNode
        interface ConditionalProcessingSVGAttributes
        interface TransformableSVGAttributes
        /// Clipping-path coordinate system: `userSpaceOnUse` or `objectBoundingBox`.
        [<Erase>]
        member _.clipPathUnits
            with set (_: string) = ()
        /// URL reference to a clipping path, commonly `url(#clip-id)`.
        [<Erase>]
        member _.``clip-path``
            with set (_: string) = ()

    /// Stores reusable graphics definitions for later reference.
    [<Erase>]
    type defs() =
        interface RegularNode
        interface ContainerElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface TransformableSVGAttributes

    /// Provides a text description of an SVG graphic for accessibility.
    [<Erase>]
    type desc() =
        interface RegularNode

    /// Draws an ellipse.
    [<Erase>]
    type ellipse() =
        interface RegularNode
        interface GraphicsElementSVGAttributes
        interface ShapeElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface TransformableSVGAttributes
        /// Horizontal center coordinate of a circle, ellipse, or radial gradient.
        [<Erase>]
        member _.cx
            with set (_: string) = ()
        /// Vertical center coordinate of a circle, ellipse, or radial gradient.
        [<Erase>]
        member _.cy
            with set (_: string) = ()
        /// Horizontal radius of an ellipse or rounded rectangle.
        [<Erase>]
        member _.rx
            with set (_: string) = ()
        /// Vertical radius of an ellipse or rounded rectangle.
        [<Erase>]
        member _.ry
            with set (_: string) = ()

    /// Combines two filter inputs using a blend mode.
    [<Erase>]
    type feBlend() =
        interface RegularNode
        interface FilterPrimitiveElementSVGAttributes
        interface DoubleInputFilterSVGAttributes
        /// Blend operation, for example `normal`, `multiply`, `screen`, `darken`, or `lighten`.
        [<Erase>]
        member _.mode
            with set (_: string) = ()

    /// Applies a color matrix transformation to a filter input.
    [<Erase>]
    type feColorMatrix() =
        interface RegularNode
        interface FilterPrimitiveElementSVGAttributes
        interface SingleInputFilterSVGAttributes
        /// Selects an SVG operation or value interpretation; allowed values depend on the element.
        [<Erase>]
        member _.type'
            with set (_: string) = ()
        /// Numeric parameters for the selected color matrix operation; `matrix` requires 20 whitespace- or comma-separated numbers.
        [<Erase>]
        member _.values
            with set (_: string) = ()

    /// Applies channel-by-channel transfer functions to a filter input.
    [<Erase>]
    type feComponentTransfer() =
        interface RegularNode
        interface FilterPrimitiveElementSVGAttributes
        interface SingleInputFilterSVGAttributes

    /// Combines two filter inputs using a compositing operation.
    [<Erase>]
    type feComposite() =
        interface RegularNode
        interface FilterPrimitiveElementSVGAttributes
        interface DoubleInputFilterSVGAttributes
        /// Filter compositing operation such as `over`, `in`, `out`, `atop`, `xor`, or `arithmetic`.
        [<Erase>]
        member _.operator
            with set (_: string) = ()
        /// First coefficient for the `arithmetic` composite operation.
        [<Erase>]
        member _.k1
            with set (_: string) = ()
        /// Second coefficient for the `arithmetic` composite operation.
        [<Erase>]
        member _.k2
            with set (_: string) = ()
        /// Third coefficient for the `arithmetic` composite operation.
        [<Erase>]
        member _.k3
            with set (_: string) = ()
        /// Fourth coefficient for the `arithmetic` composite operation.
        [<Erase>]
        member _.k4
            with set (_: string) = ()

    /// Applies a convolution matrix to a filter input.
    [<Erase>]
    type feConvolveMatrix() =
        interface RegularNode
        interface FilterPrimitiveElementSVGAttributes
        interface SingleInputFilterSVGAttributes
        /// Convolution matrix dimensions, given as one or two positive integers.
        [<Erase>]
        member _.order
            with set (_: string) = ()
        /// Space-separated convolution kernel coefficients.
        [<Erase>]
        member _.kernelMatrix
            with set (_: string) = ()
        /// Value by which convolution output is divided.
        [<Erase>]
        member _.divisor
            with set (_: string) = ()
        /// Constant added to convolution output after division.
        [<Erase>]
        member _.bias
            with set (_: string) = ()
        /// Horizontal kernel cell aligned with the output pixel.
        [<Erase>]
        member _.targetX
            with set (_: string) = ()
        /// Vertical kernel cell aligned with the output pixel.
        [<Erase>]
        member _.targetY
            with set (_: string) = ()
        /// Convolution edge handling: `duplicate`, `wrap`, or `none`.
        [<Erase>]
        member _.edgeMode
            with set (_: string) = ()
        /// Horizontal and optional vertical kernel spacing.
        [<Erase>]
        member _.kernelUnitLength
            with set (_: string) = ()
        /// Whether convolution preserves the source alpha channel.
        [<Erase>]
        member _.preserveAlpha
            with set (_: string) = ()

    /// Creates a diffuse lighting effect.
    [<Erase>]
    type feDiffuseLighting() =
        interface RegularNode
        interface FilterPrimitiveElementSVGAttributes
        interface SingleInputFilterSVGAttributes
        /// Color of the light used by a lighting filter.
        [<Erase>]
        member _.``lighting-color``
            with set (_: string) = ()
        /// Current color used by SVG paint properties such as `fill` and `stroke`.
        [<Erase>]
        member _.color
            with set (_: string) = ()
        /// Scale converting input alpha into surface height.
        [<Erase>]
        member _.surfaceScale
            with set (_: string) = ()
        /// Diffuse reflection constant for a lighting filter.
        [<Erase>]
        member _.diffuseConstant
            with set (_: string) = ()
        /// Horizontal and optional vertical kernel spacing.
        [<Erase>]
        member _.kernelUnitLength
            with set (_: string) = ()

    /// Displaces pixels using a second filter input.
    [<Erase>]
    type feDisplacementMap() =
        interface RegularNode
        interface FilterPrimitiveElementSVGAttributes
        interface DoubleInputFilterSVGAttributes
        /// Displacement amount used by a displacement-map filter.
        [<Erase>]
        member _.scale
            with set (_: string) = ()
        /// Displacement-map x channel: `R`, `G`, `B`, or `A`.
        [<Erase>]
        member _.xChannelSelector
            with set (_: string) = ()
        /// Displacement-map y channel: `R`, `G`, `B`, or `A`.
        [<Erase>]
        member _.yChannelSelector
            with set (_: string) = ()

    /// Defines a distant light source for lighting filters.
    [<Erase>]
    type feDistantLight() =
        interface RegularNode
        interface LightSourceElementSVGAttributes
        /// Light direction angle in the x-y plane, in degrees.
        [<Erase>]
        member _.azimuth
            with set (_: string) = ()
        /// Light elevation angle above the x-y plane, in degrees.
        [<Erase>]
        member _.elevation
            with set (_: string) = ()

    /// Adds a drop-shadow filter effect.
    [<Erase>]
    type feDropShadow() =
        interface RegularNode
        interface FilterPrimitiveElementSVGAttributes
        /// Current color used by SVG paint properties such as `fill` and `stroke`.
        [<Erase>]
        member _.color
            with set (_: string) = ()
        /// Color produced by a flood filter primitive.
        [<Erase>]
        member _.``flood-color``
            with set (_: string) = ()
        /// Flood opacity from `0` to `1`.
        [<Erase>]
        member _.``flood-opacity``
            with set (_: string) = ()
        /// Horizontal offset or displacement, depending on the SVG element.
        [<Erase>]
        member _.dx
            with set (_: string) = ()
        /// Vertical offset or displacement, depending on the SVG element.
        [<Erase>]
        member _.dy
            with set (_: string) = ()
        /// Gaussian blur standard deviation; may specify horizontal and vertical values.
        [<Erase>]
        member _.stdDeviation
            with set (_: string) = ()

    /// Fills a filter region with a color and opacity.
    [<Erase>]
    type feFlood() =
        interface RegularNode
        interface FilterPrimitiveElementSVGAttributes
        /// Current color used by SVG paint properties such as `fill` and `stroke`.
        [<Erase>]
        member _.color
            with set (_: string) = ()
        /// Color produced by a flood filter primitive.
        [<Erase>]
        member _.``flood-color``
            with set (_: string) = ()
        /// Flood opacity from `0` to `1`.
        [<Erase>]
        member _.``flood-opacity``
            with set (_: string) = ()

    /// Defines an alpha-channel transfer function.
    [<Erase>]
    type feFuncA() =
        interface RegularNode
        /// Selects an SVG operation or value interpretation; allowed values depend on the element.
        [<Erase>]
        member _.type'
            with set (_: string) = ()
        /// Space-separated lookup values for a component transfer function.
        [<Erase>]
        member _.tableValues
            with set (_: string) = ()
        /// Slope of a component transfer function.
        [<Erase>]
        member _.slope
            with set (_: string) = ()
        /// Intercept of a component transfer function.
        [<Erase>]
        member _.intercept
            with set (_: string) = ()
        /// Amplitude used by an amplitude transfer function.
        [<Erase>]
        member _.amplitude
            with set (_: string) = ()
        /// Exponent used by an exponent transfer function.
        [<Erase>]
        member _.exponent
            with set (_: string) = ()
        /// Position or distance offset, depending on the SVG element.
        [<Erase>]
        member _.offset
            with set (_: string) = ()

    /// Defines a blue-channel transfer function.
    [<Erase>]
    type feFuncB() =
        interface RegularNode
        /// Selects an SVG operation or value interpretation; allowed values depend on the element.
        [<Erase>]
        member _.type'
            with set (_: string) = ()
        /// Space-separated lookup values for a component transfer function.
        [<Erase>]
        member _.tableValues
            with set (_: string) = ()
        /// Slope of a component transfer function.
        [<Erase>]
        member _.slope
            with set (_: string) = ()
        /// Intercept of a component transfer function.
        [<Erase>]
        member _.intercept
            with set (_: string) = ()
        /// Amplitude used by an amplitude transfer function.
        [<Erase>]
        member _.amplitude
            with set (_: string) = ()
        /// Exponent used by an exponent transfer function.
        [<Erase>]
        member _.exponent
            with set (_: string) = ()
        /// Position or distance offset, depending on the SVG element.
        [<Erase>]
        member _.offset
            with set (_: string) = ()

    /// Defines a green-channel transfer function.
    [<Erase>]
    type feFuncG() =
        interface RegularNode
        /// Selects an SVG operation or value interpretation; allowed values depend on the element.
        [<Erase>]
        member _.type'
            with set (_: string) = ()
        /// Space-separated lookup values for a component transfer function.
        [<Erase>]
        member _.tableValues
            with set (_: string) = ()
        /// Slope of a component transfer function.
        [<Erase>]
        member _.slope
            with set (_: string) = ()
        /// Intercept of a component transfer function.
        [<Erase>]
        member _.intercept
            with set (_: string) = ()
        /// Amplitude used by an amplitude transfer function.
        [<Erase>]
        member _.amplitude
            with set (_: string) = ()
        /// Exponent used by an exponent transfer function.
        [<Erase>]
        member _.exponent
            with set (_: string) = ()
        /// Position or distance offset, depending on the SVG element.
        [<Erase>]
        member _.offset
            with set (_: string) = ()

    /// Defines a red-channel transfer function.
    [<Erase>]
    type feFuncR() =
        interface RegularNode
        /// Selects an SVG operation or value interpretation; allowed values depend on the element.
        [<Erase>]
        member _.type'
            with set (_: string) = ()
        /// Space-separated lookup values for a component transfer function.
        [<Erase>]
        member _.tableValues
            with set (_: string) = ()
        /// Slope of a component transfer function.
        [<Erase>]
        member _.slope
            with set (_: string) = ()
        /// Intercept of a component transfer function.
        [<Erase>]
        member _.intercept
            with set (_: string) = ()
        /// Amplitude used by an amplitude transfer function.
        [<Erase>]
        member _.amplitude
            with set (_: string) = ()
        /// Exponent used by an exponent transfer function.
        [<Erase>]
        member _.exponent
            with set (_: string) = ()
        /// Position or distance offset, depending on the SVG element.
        [<Erase>]
        member _.offset
            with set (_: string) = ()

    /// Applies a Gaussian blur to a filter input.
    [<Erase>]
    type feGaussianBlur() =
        interface RegularNode
        interface FilterPrimitiveElementSVGAttributes
        interface SingleInputFilterSVGAttributes
        /// Gaussian blur standard deviation; may specify horizontal and vertical values.
        [<Erase>]
        member _.stdDeviation
            with set (_: string) = ()

    /// Uses an image or SVG element as a filter input.
    [<Erase>]
    type feImage() =
        interface RegularNode
        interface FilterPrimitiveElementSVGAttributes
        /// How the viewBox is fitted into the viewport and whether its aspect ratio is preserved.
        [<Erase>]
        member _.preserveAspectRatio
            with set (_: string) = ()
        /// URL reference to another SVG resource or element.
        [<Erase>]
        member _.href
            with set (_: string) = ()

    /// Combines multiple filter inputs in sequence.
    [<Erase>]
    type feMerge() =
        interface RegularNode
        interface FilterPrimitiveElementSVGAttributes

    /// Adds one filter input to an `feMerge` result.
    [<Erase>]
    type feMergeNode() =
        interface VoidNode
        interface SingleInputFilterSVGAttributes

    /// Erodes or dilates a filter input.
    [<Erase>]
    type feMorphology() =
        interface RegularNode
        interface FilterPrimitiveElementSVGAttributes
        interface SingleInputFilterSVGAttributes
        /// Morphology operation: `erode` or `dilate`.
        [<Erase>]
        member _.operator
            with set (_: string) = ()
        /// Radius used by the SVG morphology filter to erode or dilate its input.
        [<Erase>]
        member _.radius
            with set (_: string) = ()

    /// Offsets a filter input.
    [<Erase>]
    type feOffset() =
        interface RegularNode
        interface FilterPrimitiveElementSVGAttributes
        interface SingleInputFilterSVGAttributes
        /// Horizontal offset or displacement, depending on the SVG element.
        [<Erase>]
        member _.dx
            with set (_: string) = ()
        /// Vertical offset or displacement, depending on the SVG element.
        [<Erase>]
        member _.dy
            with set (_: string) = ()

    /// Defines a point light source for lighting filters.
    [<Erase>]
    type fePointLight() =
        interface RegularNode
        interface LightSourceElementSVGAttributes
        /// Horizontal position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.x
            with set (_: string) = ()
        /// Vertical position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.y
            with set (_: string) = ()
        /// Depth coordinate in the SVG user coordinate system.
        [<Erase>]
        member _.z
            with set (_: string) = ()

    /// Creates a specular lighting effect.
    [<Erase>]
    type feSpecularLighting() =
        interface RegularNode
        interface FilterPrimitiveElementSVGAttributes
        interface SingleInputFilterSVGAttributes
        /// Color of the light used by a lighting filter.
        [<Erase>]
        member _.``lighting-color``
            with set (_: string) = ()
        /// Current color used by SVG paint properties such as `fill` and `stroke`.
        [<Erase>]
        member _.color
            with set (_: string) = ()
        /// Scale converting input alpha into surface height.
        [<Erase>]
        member _.surfaceScale
            with set (_: string) = ()
        /// Specular reflection constant for a lighting filter.
        [<Erase>]
        member _.specularConstant
            with set (_: string) = ()
        /// Shininess exponent for a specular lighting filter.
        [<Erase>]
        member _.specularExponent
            with set (_: string) = ()
        /// Horizontal and optional vertical kernel spacing.
        [<Erase>]
        member _.kernelUnitLength
            with set (_: string) = ()

    /// Defines a spotlight source for lighting filters.
    [<Erase>]
    type feSpotLight() =
        interface RegularNode
        interface LightSourceElementSVGAttributes
        /// Horizontal position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.x
            with set (_: string) = ()
        /// Vertical position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.y
            with set (_: string) = ()
        /// Depth coordinate in the SVG user coordinate system.
        [<Erase>]
        member _.z
            with set (_: string) = ()
        /// Horizontal coordinate targeted by a spotlight.
        [<Erase>]
        member _.pointsAtX
            with set (_: string) = ()
        /// Vertical coordinate targeted by a spotlight.
        [<Erase>]
        member _.pointsAtY
            with set (_: string) = ()
        /// Depth coordinate targeted by a spotlight.
        [<Erase>]
        member _.pointsAtZ
            with set (_: string) = ()
        /// Shininess exponent for a specular lighting filter.
        [<Erase>]
        member _.specularExponent
            with set (_: string) = ()
        /// Maximum spotlight cone angle in degrees.
        [<Erase>]
        member _.limitingConeAngle
            with set (_: string) = ()

    /// Repeats a filter input across a filter region.
    [<Erase>]
    type feTile() =
        interface RegularNode
        interface FilterPrimitiveElementSVGAttributes
        interface SingleInputFilterSVGAttributes

    /// Generates turbulence or fractal-noise texture.
    [<Erase>]
    type feTurbulence() =
        interface RegularNode
        interface FilterPrimitiveElementSVGAttributes
        /// Base frequency of turbulence noise, as one or two numbers.
        [<Erase>]
        member _.baseFrequency
            with set (_: string) = ()
        /// Number of turbulence octaves.
        [<Erase>]
        member _.numOctaves
            with set (_: string) = ()
        /// Seed used to initialize turbulence noise.
        [<Erase>]
        member _.seed
            with set (_: string) = ()
        /// Whether turbulence tiles seamlessly: `stitch` or `noStitch`.
        [<Erase>]
        member _.stitchTiles
            with set (_: string) = ()
        /// Selects an SVG operation or value interpretation; allowed values depend on the element.
        [<Erase>]
        member _.type'
            with set (_: string) = ()

    /// Defines a filter effect that can be applied to SVG graphics.
    [<Erase>]
    type filter() =
        interface RegularNode
        /// Filter-region coordinate system: `userSpaceOnUse` or `objectBoundingBox`.
        [<Erase>]
        member _.filterUnits
            with set (_: string) = ()
        /// Filter-primitive coordinate system: `userSpaceOnUse` or `objectBoundingBox`.
        [<Erase>]
        member _.primitiveUnits
            with set (_: string) = ()
        /// Horizontal position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.x
            with set (_: string) = ()
        /// Vertical position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.y
            with set (_: string) = ()
        /// Width of an SVG viewport, shape, filter region, or primitive.
        [<Erase>]
        member _.width
            with set (_: string) = ()
        /// Height of an SVG viewport, shape, filter region, or primitive.
        [<Erase>]
        member _.height
            with set (_: string) = ()
        /// Filter resolution as one or two positive integers.
        [<Erase>]
        member _.filterRes
            with set (_: string) = ()

    /// Embeds content from another XML namespace, commonly HTML.
    [<Erase>]
    type foreignObject() =
        interface RegularNode
        interface NewViewportSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface TransformableSVGAttributes
        /// Visibility state: `visible`, `hidden`, or `collapse`.
        [<Erase>]
        member _.visibility
            with set (_: string) = ()
        /// Whether the element participates in rendering; `none` hides it.
        [<Erase>]
        member _.display
            with set (_: string) = ()
        /// Horizontal position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.x
            with set (_: string) = ()
        /// Vertical position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.y
            with set (_: string) = ()
        /// Width of an SVG viewport, shape, filter region, or primitive.
        [<Erase>]
        member _.width
            with set (_: string) = ()
        /// Height of an SVG viewport, shape, filter region, or primitive.
        [<Erase>]
        member _.height
            with set (_: string) = ()

    /// Groups SVG elements so they can share transformations and presentation attributes.
    [<Erase>]
    type g() =
        interface RegularNode
        interface ContainerElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface TransformableSVGAttributes
        /// Visibility state: `visible`, `hidden`, or `collapse`.
        [<Erase>]
        member _.visibility
            with set (_: string) = ()
        /// Whether the element participates in rendering; `none` hides it.
        [<Erase>]
        member _.display
            with set (_: string) = ()

    /// Embeds an external raster image in SVG.
    [<Erase>]
    type image() =
        interface RegularNode
        interface NewViewportSVGAttributes
        interface GraphicsElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface TransformableSVGAttributes
        /// Image rendering hint: `auto`, `optimizeSpeed`, or `optimizeQuality`.
        [<Erase>]
        member _.``image-rendering``
            with set (_: string) = ()
        /// Legacy color profile hint for rendering an image.
        [<Erase>]
        member _.``color-profile``
            with set (_: string) = ()
        /// Horizontal position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.x
            with set (_: string) = ()
        /// Vertical position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.y
            with set (_: string) = ()
        /// Width of an SVG viewport, shape, filter region, or primitive.
        [<Erase>]
        member _.width
            with set (_: string) = ()
        /// Height of an SVG viewport, shape, filter region, or primitive.
        [<Erase>]
        member _.height
            with set (_: string) = ()
        /// How the viewBox is fitted into the viewport and whether its aspect ratio is preserved.
        [<Erase>]
        member _.preserveAspectRatio
            with set (_: string) = ()
        /// URL reference to another SVG resource or element.
        [<Erase>]
        member _.href
            with set (_: string) = ()

    /// Draws a straight line between two points.
    [<Erase>]
    type line() =
        interface RegularNode
        interface GraphicsElementSVGAttributes
        interface ShapeElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface TransformableSVGAttributes
        /// Marker placed at the final vertex of a path.
        [<Erase>]
        member _.``marker-end``
            with set (_: string) = ()
        /// Marker placed at interior vertices of a path.
        [<Erase>]
        member _.``marker-mid``
            with set (_: string) = ()
        /// Marker placed at the initial vertex of a path.
        [<Erase>]
        member _.``marker-start``
            with set (_: string) = ()
        /// Horizontal coordinate of the first endpoint or linear-gradient start.
        [<Erase>]
        member _.x1
            with set (_: string) = ()
        /// Vertical coordinate of the first endpoint or linear-gradient start.
        [<Erase>]
        member _.y1
            with set (_: string) = ()
        /// Horizontal coordinate of the second endpoint or linear-gradient end.
        [<Erase>]
        member _.x2
            with set (_: string) = ()
        /// Vertical coordinate of the second endpoint or linear-gradient end.
        [<Erase>]
        member _.y2
            with set (_: string) = ()

    /// Defines a color gradient along a straight line.
    [<Erase>]
    type linearGradient() =
        interface RegularNode
        interface GradientElementSVGAttributes
        /// Horizontal coordinate of the first endpoint or linear-gradient start.
        [<Erase>]
        member _.x1
            with set (_: string) = ()
        /// Horizontal coordinate of the second endpoint or linear-gradient end.
        [<Erase>]
        member _.x2
            with set (_: string) = ()
        /// Vertical coordinate of the first endpoint or linear-gradient start.
        [<Erase>]
        member _.y1
            with set (_: string) = ()
        /// Vertical coordinate of the second endpoint or linear-gradient end.
        [<Erase>]
        member _.y2
            with set (_: string) = ()

    /// Defines a graphic used to decorate path vertices or endpoints.
    [<Erase>]
    type marker() =
        interface RegularNode
        interface ContainerElementSVGAttributes
        interface FitToViewBoxSVGAttributes
        /// Legacy clipping rectangle; prefer `clip-path`.
        [<Erase>]
        member _.clip
            with set (_: string) = ()
        /// Handling of overflowing content: `visible`, `hidden`, `scroll`, or `auto`.
        [<Erase>]
        member _.overflow
            with set (_: string) = ()
        /// Marker sizing mode: `strokeWidth` or `userSpaceOnUse`.
        [<Erase>]
        member _.markerUnits
            with set (_: string) = ()
        /// Horizontal marker reference point aligned to the vertex.
        [<Erase>]
        member _.refX
            with set (_: string) = ()
        /// Vertical marker reference point aligned to the vertex.
        [<Erase>]
        member _.refY
            with set (_: string) = ()
        /// Width of the marker viewport.
        [<Erase>]
        member _.markerWidth
            with set (_: string) = ()
        /// Height of the marker viewport.
        [<Erase>]
        member _.markerHeight
            with set (_: string) = ()
        /// Marker orientation: an angle, `auto`, or `auto-start-reverse`.
        [<Erase>]
        member _.orient
            with set (_: string) = ()

    /// Defines a transparency mask for SVG graphics.
    [<Erase>]
    type mask() =
        interface RegularNode
        interface ConditionalProcessingSVGAttributes
        /// URL reference to an SVG filter, commonly `url(#filter-id)`.
        [<Erase>]
        member _.filter
            with set (_: string) = ()
        /// Overall opacity from `0` (transparent) to `1` (opaque).
        [<Erase>]
        member _.opacity
            with set (_: string) = ()
        /// Mask-region coordinate system: `userSpaceOnUse` or `objectBoundingBox`.
        [<Erase>]
        member _.maskUnits
            with set (_: string) = ()
        /// Mask-content coordinate system: `userSpaceOnUse` or `objectBoundingBox`.
        [<Erase>]
        member _.maskContentUnits
            with set (_: string) = ()
        /// Horizontal position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.x
            with set (_: string) = ()
        /// Vertical position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.y
            with set (_: string) = ()
        /// Width of an SVG viewport, shape, filter region, or primitive.
        [<Erase>]
        member _.width
            with set (_: string) = ()
        /// Height of an SVG viewport, shape, filter region, or primitive.
        [<Erase>]
        member _.height
            with set (_: string) = ()

    /// Stores structured metadata about an SVG document.
    [<Erase>]
    type metadata() =
        interface RegularNode

    /// References a motion path for SVG animation.
    [<Erase>]
    type mpath() =
        interface VoidNode

    /// Draws a shape from SVG path commands.
    [<Erase>]
    type path() =
        interface RegularNode
        interface GraphicsElementSVGAttributes
        interface ShapeElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface TransformableSVGAttributes
        /// Marker placed at the final vertex of a path.
        [<Erase>]
        member _.``marker-end``
            with set (_: string) = ()
        /// Marker placed at interior vertices of a path.
        [<Erase>]
        member _.``marker-mid``
            with set (_: string) = ()
        /// Marker placed at the initial vertex of a path.
        [<Erase>]
        member _.``marker-start``
            with set (_: string) = ()
        /// SVG path commands and coordinates describing the path outline.
        [<Erase>]
        member _.d
            with set (_: string) = ()
        /// Author-provided total path length used to calibrate distance calculations.
        [<Erase>]
        member _.pathLength
            with set (_: string) = ()

    /// Defines a repeating graphic pattern used as paint.
    [<Erase>]
    type pattern() =
        interface RegularNode
        interface ContainerElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface FitToViewBoxSVGAttributes
        /// Horizontal position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.x
            with set (_: string) = ()
        /// Vertical position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.y
            with set (_: string) = ()
        /// Width of an SVG viewport, shape, filter region, or primitive.
        [<Erase>]
        member _.width
            with set (_: string) = ()
        /// Height of an SVG viewport, shape, filter region, or primitive.
        [<Erase>]
        member _.height
            with set (_: string) = ()
        /// Pattern placement coordinate system: `userSpaceOnUse` or `objectBoundingBox`.
        [<Erase>]
        member _.patternUnits
            with set (_: string) = ()
        /// Pattern-content coordinate system: `userSpaceOnUse` or `objectBoundingBox`.
        [<Erase>]
        member _.patternContentUnits
            with set (_: string) = ()
        /// Transformation applied to the pattern coordinate system.
        [<Erase>]
        member _.patternTransform
            with set (_: string) = ()
        /// URL reference to another SVG resource or element.
        [<Erase>]
        member _.href
            with set (_: string) = ()
        /// Legacy clipping rectangle; prefer `clip-path`.
        [<Erase>]
        member _.clip
            with set (_: string) = ()
        /// Handling of overflowing content: `visible`, `hidden`, `scroll`, or `auto`.
        [<Erase>]
        member _.overflow
            with set (_: string) = ()

    /// Draws a closed shape from a sequence of points.
    [<Erase>]
    type polygon() =
        interface RegularNode
        interface GraphicsElementSVGAttributes
        interface ShapeElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface TransformableSVGAttributes
        /// Marker placed at the final vertex of a path.
        [<Erase>]
        member _.``marker-end``
            with set (_: string) = ()
        /// Marker placed at interior vertices of a path.
        [<Erase>]
        member _.``marker-mid``
            with set (_: string) = ()
        /// Marker placed at the initial vertex of a path.
        [<Erase>]
        member _.``marker-start``
            with set (_: string) = ()
        /// Coordinate pairs defining polygon or polyline vertices.
        [<Erase>]
        member _.points
            with set (_: string) = ()

    /// Draws connected line segments from a sequence of points.
    [<Erase>]
    type polyline() =
        interface RegularNode
        interface GraphicsElementSVGAttributes
        interface ShapeElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface TransformableSVGAttributes
        /// Marker placed at the final vertex of a path.
        [<Erase>]
        member _.``marker-end``
            with set (_: string) = ()
        /// Marker placed at interior vertices of a path.
        [<Erase>]
        member _.``marker-mid``
            with set (_: string) = ()
        /// Marker placed at the initial vertex of a path.
        [<Erase>]
        member _.``marker-start``
            with set (_: string) = ()
        /// Coordinate pairs defining polygon or polyline vertices.
        [<Erase>]
        member _.points
            with set (_: string) = ()

    /// Defines a circular or elliptical color gradient.
    [<Erase>]
    type radialGradient() =
        interface RegularNode
        interface GradientElementSVGAttributes
        /// Horizontal center coordinate of a circle, ellipse, or radial gradient.
        [<Erase>]
        member _.cx
            with set (_: string) = ()
        /// Vertical center coordinate of a circle, ellipse, or radial gradient.
        [<Erase>]
        member _.cy
            with set (_: string) = ()
        /// Radius of a circle or radial gradient.
        [<Erase>]
        member _.r
            with set (_: string) = ()
        /// Horizontal focal point of a radial gradient.
        [<Erase>]
        member _.fx
            with set (_: string) = ()
        /// Vertical focal point of a radial gradient.
        [<Erase>]
        member _.fy
            with set (_: string) = ()

    /// Draws a rectangle, optionally with rounded corners.
    [<Erase>]
    type rect() =
        interface RegularNode
        interface GraphicsElementSVGAttributes
        interface ShapeElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface TransformableSVGAttributes
        /// Horizontal position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.x
            with set (_: string) = ()
        /// Vertical position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.y
            with set (_: string) = ()
        /// Width of an SVG viewport, shape, filter region, or primitive.
        [<Erase>]
        member _.width
            with set (_: string) = ()
        /// Height of an SVG viewport, shape, filter region, or primitive.
        [<Erase>]
        member _.height
            with set (_: string) = ()
        /// Horizontal radius of an ellipse or rounded rectangle.
        [<Erase>]
        member _.rx
            with set (_: string) = ()
        /// Vertical radius of an ellipse or rounded rectangle.
        [<Erase>]
        member _.ry
            with set (_: string) = ()

    /// Sets an attribute value for a specified animation interval.
    [<Erase>]
    type set() =
        interface RegularNode
        interface AnimationTimingSVGAttributes

    /// Defines a color and opacity stop in a gradient.
    [<Erase>]
    type stop() =
        interface VoidNode
        /// Current color used by SVG paint properties such as `fill` and `stroke`.
        [<Erase>]
        member _.color
            with set (_: string) = ()
        /// Color at a gradient stop.
        [<Erase>]
        member _.``stop-color``
            with set (_: string) = ()
        /// Gradient stop opacity from `0` to `1`.
        [<Erase>]
        member _.``stop-opacity``
            with set (_: string) = ()
        /// Position or distance offset, depending on the SVG element.
        [<Erase>]
        member _.offset
            with set (_: string) = ()

    /// Creates an SVG viewport and coordinate system.
    [<Erase>]
    type svg() =
        interface RegularNode
        interface ContainerElementSVGAttributes
        interface NewViewportSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface FitToViewBoxSVGAttributes
        interface ZoomAndPanSVGAttributes
        interface PresentationSVGAttributes
        /// Legacy SVG specification version.
        [<Erase>]
        member _.version
            with set (_: string) = ()
        /// Legacy SVG profile identifier.
        [<Erase>]
        member _.baseProfile
            with set (_: string) = ()
        /// Horizontal position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.x
            with set (_: string) = ()
        /// Vertical position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.y
            with set (_: string) = ()
        /// Width of an SVG viewport, shape, filter region, or primitive.
        [<Erase>]
        member _.width
            with set (_: string) = ()
        /// Height of an SVG viewport, shape, filter region, or primitive.
        [<Erase>]
        member _.height
            with set (_: string) = ()
        /// Legacy scripting language MIME type declaration.
        [<Erase>]
        member _.contentScriptType
            with set (_: string) = ()
        /// Legacy style language MIME type declaration.
        [<Erase>]
        member _.contentStyleType
            with set (_: string) = ()
        /// XML namespace URI; for SVG use `http://www.w3.org/2000/svg`.
        [<Erase>]
        member _.xmlns
            with set (_: string) = ()

    /// Selects the first child whose conditional-processing requirements are satisfied.
    [<Erase>]
    type switch() =
        interface RegularNode
        interface ContainerElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface TransformableSVGAttributes
        /// Whether the element participates in rendering; `none` hides it.
        [<Erase>]
        member _.display
            with set (_: string) = ()
        /// Visibility state: `visible`, `hidden`, or `collapse`.
        [<Erase>]
        member _.visibility
            with set (_: string) = ()

    /// Defines a reusable graphical template, commonly rendered through `use`.
    [<Erase>]
    type symbol() =
        interface RegularNode
        interface ContainerElementSVGAttributes
        interface NewViewportSVGAttributes
        interface FitToViewBoxSVGAttributes
        /// Width of an SVG viewport, shape, filter region, or primitive.
        [<Erase>]
        member _.width
            with set (_: string) = ()
        /// Height of an SVG viewport, shape, filter region, or primitive.
        [<Erase>]
        member _.height
            with set (_: string) = ()
        /// How the viewBox is fitted into the viewport and whether its aspect ratio is preserved.
        [<Erase>]
        member _.preserveAspectRatio
            with set (_: string) = ()
        /// Horizontal reference point used to position instances of the symbol.
        [<Erase>]
        member _.refX
            with set (_: string) = ()
        /// Vertical reference point used to position instances of the symbol.
        [<Erase>]
        member _.refY
            with set (_: string) = ()
        /// Four numbers defining the SVG view rectangle: minimum x, minimum y, width, and height.
        [<Erase>]
        member _.viewBox
            with set (_: string) = ()
        /// Horizontal position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.x
            with set (_: string) = ()
        /// Vertical position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.y
            with set (_: string) = ()

    /// Creates a text string in SVG.
    [<Erase>]
    type text() =
        interface RegularNode
        interface TextContentElementSVGAttributes
        interface GraphicsElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface TransformableSVGAttributes
        /// Horizontal position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.x
            with set (_: string) = ()
        /// Vertical position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.y
            with set (_: string) = ()
        /// Horizontal offset or displacement, depending on the SVG element.
        [<Erase>]
        member _.dx
            with set (_: string) = ()
        /// Vertical offset or displacement, depending on the SVG element.
        [<Erase>]
        member _.dy
            with set (_: string) = ()
        /// List of rotation angles applied to successive glyphs.
        [<Erase>]
        member _.rotate
            with set (_: string) = ()
        /// Target rendered length of the text string.
        [<Erase>]
        member _.textLength
            with set (_: string) = ()
        /// Text adjustment strategy: `spacing` or `spacingAndGlyphs`.
        [<Erase>]
        member _.lengthAdjust
            with set (_: string) = ()
        /// Text writing direction and line progression.
        [<Erase>]
        member _.``writing-mode``
            with set (_: string) = ()
        /// Hint for balancing text rendering speed and legibility.
        [<Erase>]
        member _.``text-rendering``
            with set (_: string) = ()

    /// Positions text along a referenced path.
    [<Erase>]
    type textPath() =
        interface RegularNode
        interface TextContentElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        /// Offset from the start of the referenced text path.
        [<Erase>]
        member _.startOffset
            with set (_: string) = ()
        /// Text-on-path method: `align` or `stretch`.
        [<Erase>]
        member _.method
            with set (_: string) = ()
        /// Text-on-path spacing: `auto` or `exact`.
        [<Erase>]
        member _.spacing
            with set (_: string) = ()
        /// Baseline of this element aligned with its parent text run.
        [<Erase>]
        member _.``alignment-baseline``
            with set (_: string) = ()
        /// Shifts the dominant baseline by a length or percentage.
        [<Erase>]
        member _.``baseline-shift``
            with set (_: string) = ()
        /// Whether the element participates in rendering; `none` hides it.
        [<Erase>]
        member _.display
            with set (_: string) = ()
        /// Visibility state: `visible`, `hidden`, or `collapse`.
        [<Erase>]
        member _.visibility
            with set (_: string) = ()
        /// URL reference to another SVG resource or element.
        [<Erase>]
        member _.href
            with set (_: string) = ()

    /// Marks up a subrange of SVG text for positioning or styling.
    [<Erase>]
    type tspan() =
        interface RegularNode
        interface TextContentElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        /// Horizontal position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.x
            with set (_: string) = ()
        /// Vertical position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.y
            with set (_: string) = ()
        /// Horizontal offset or displacement, depending on the SVG element.
        [<Erase>]
        member _.dx
            with set (_: string) = ()
        /// Vertical offset or displacement, depending on the SVG element.
        [<Erase>]
        member _.dy
            with set (_: string) = ()
        /// List of rotation angles applied to successive glyphs.
        [<Erase>]
        member _.rotate
            with set (_: string) = ()
        /// Target rendered length of the text string.
        [<Erase>]
        member _.textLength
            with set (_: string) = ()
        /// Text adjustment strategy: `spacing` or `spacingAndGlyphs`.
        [<Erase>]
        member _.lengthAdjust
            with set (_: string) = ()
        /// Baseline of this element aligned with its parent text run.
        [<Erase>]
        member _.``alignment-baseline``
            with set (_: string) = ()
        /// Shifts the dominant baseline by a length or percentage.
        [<Erase>]
        member _.``baseline-shift``
            with set (_: string) = ()
        /// Whether the element participates in rendering; `none` hides it.
        [<Erase>]
        member _.display
            with set (_: string) = ()
        /// Visibility state: `visible`, `hidden`, or `collapse`.
        [<Erase>]
        member _.visibility
            with set (_: string) = ()

    /// Instantiates and renders a referenced SVG element or symbol.
    [<Erase>]
    type use'() =
        interface RegularNode
        interface ConditionalProcessingSVGAttributes
        interface GraphicsElementSVGAttributes
        interface PresentationSVGAttributes
        interface TransformableSVGAttributes
        /// Horizontal position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.x
            with set (_: string) = ()
        /// Vertical position or coordinate in the current SVG user coordinate system.
        [<Erase>]
        member _.y
            with set (_: string) = ()
        /// Width of an SVG viewport, shape, filter region, or primitive.
        [<Erase>]
        member _.width
            with set (_: string) = ()
        /// Height of an SVG viewport, shape, filter region, or primitive.
        [<Erase>]
        member _.height
            with set (_: string) = ()
        /// URL reference to another SVG resource or element.
        [<Erase>]
        member _.href
            with set (_: string) = ()

    /// Defines a reusable view of an SVG document.
    [<Erase>]
    type view() =
        interface RegularNode
        interface FitToViewBoxSVGAttributes
        interface ZoomAndPanSVGAttributes
        /// IDs of elements in the view targeted by this SVG view.
        [<Erase>]
        member _.viewTarget
            with set (_: string) = ()
