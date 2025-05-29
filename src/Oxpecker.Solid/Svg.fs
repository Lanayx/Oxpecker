namespace Oxpecker.Solid

open Fable.Core
module Svg =

    [<AllowNullLiteral>]
    [<Interface>]
    type CoreSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type ExternalResourceSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type ConditionalProcessingSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type AnimationElementSVGAttributes =
        inherit CoreSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit ConditionalProcessingSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type ShapeElementSVGAttributes =
        inherit CoreSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type ContainerElementSVGAttributes =
        inherit CoreSVGAttributes
        inherit ShapeElementSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type FilterPrimitiveElementSVGAttributes =
        inherit CoreSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type StylableSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type TransformableSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type AnimationTimingSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type AnimationValueSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type AnimationAdditionSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type AnimationAttributeTargetSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type PresentationSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type SingleInputFilterSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type DoubleInputFilterSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type FitToViewBoxSVGAttributes = interface end

    [<AllowNullLiteral>]
    [<Interface>]
    type GradientElementSVGAttributes =
        inherit CoreSVGAttributes
        inherit ExternalResourceSVGAttributes
        inherit StylableSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type GraphicsElementSVGAttributes =
        inherit CoreSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type LightSourceElementSVGAttributes =
        inherit CoreSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type NewViewportSVGAttributes =
        inherit CoreSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type TextContentElementSVGAttributes =
        inherit CoreSVGAttributes

    [<AllowNullLiteral>]
    [<Interface>]
    type ZoomAndPanSVGAttributes = interface end

    type CoreSVGAttributes with
        [<Erase>]
        member _.id
            with set (_: string) = ()
        [<Erase>]
        member _.lang
            with set (_: string) = ()
        [<Erase>]
        member _.tabIndex
            with set (_: string) = ()
        [<Erase>]
        member _.tabindex
            with set (_: string) = ()

    type StylableSVGAttributes with
        [<Erase>]
        member _.class'
            with set (_: string) = ()
        [<Erase>]
        member _.style
            with set (_: string) = ()

    type TransformableSVGAttributes with
        [<Erase>]
        member _.transform
            with set (_: string) = ()

    type ConditionalProcessingSVGAttributes with
        [<Erase>]
        member _.requiredExtensions
            with set (_: string) = ()
        [<Erase>]
        member _.requiredFeatures
            with set (_: string) = ()
        [<Erase>]
        member _.systemLanguage
            with set (_: string) = ()

    type ExternalResourceSVGAttributes with
        [<Erase>]
        member _.externalResourcesRequired
            with set (_: string) = ()

    type AnimationTimingSVGAttributes with
        [<Erase>]
        member _.begin'
            with set (_: string) = ()
        [<Erase>]
        member _.dur
            with set (_: string) = ()
        [<Erase>]
        member _.end'
            with set (_: string) = ()
        [<Erase>]
        member _.min
            with set (_: string) = ()
        [<Erase>]
        member _.max
            with set (_: string) = ()
        [<Erase>]
        member _.restart
            with set (_: string) = ()
        [<Erase>]
        member _.repeatCount
            with set (_: string) = ()
        [<Erase>]
        member _.repeatDur
            with set (_: string) = ()
        [<Erase>]
        member _.fill
            with set (_: string) = ()

    type AnimationValueSVGAttributes with
        [<Erase>]
        member _.calcMode
            with set (_: string) = ()
        [<Erase>]
        member _.values
            with set (_: string) = ()
        [<Erase>]
        member _.keyTimes
            with set (_: string) = ()
        [<Erase>]
        member _.keySplines
            with set (_: string) = ()
        [<Erase>]
        member _.from
            with set (_: string) = ()
        [<Erase>]
        member _.to'
            with set (_: string) = ()
        [<Erase>]
        member _.by
            with set (_: string) = ()

    type AnimationAdditionSVGAttributes with
        [<Erase>]
        member _.attributeName
            with set (_: string) = ()
        [<Erase>]
        member _.additive
            with set (_: string) = ()
        [<Erase>]
        member _.accumulate
            with set (_: string) = ()

    type AnimationAttributeTargetSVGAttributes with
        [<Erase>]
        member _.attributeName
            with set (_: string) = ()
        [<Erase>]
        member _.attributeType
            with set (_: string) = ()

    type ContainerElementSVGAttributes with
        [<Erase>]
        member _.``color-rendering``
            with set (_: string) = ()
        [<Erase>]
        member _.``clip-path``
            with set (_: string) = ()
        [<Erase>]
        member _.cursor
            with set (_: string) = ()
        [<Erase>]
        member _.``color-interpolation``
            with set (_: string) = ()
        [<Erase>]
        member _.``enable-background``
            with set (_: string) = ()
        [<Erase>]
        member _.filter
            with set (_: string) = ()
        [<Erase>]
        member _.mask
            with set (_: string) = ()
        [<Erase>]
        member _.opacity
            with set (_: string) = ()

    type GraphicsElementSVGAttributes with
        [<Erase>]
        member _.``clip-rule``
            with set (_: string) = ()
        [<Erase>]
        member _.mask
            with set (_: string) = ()
        [<Erase>]
        member _.``pointer-events``
            with set (_: string) = ()
        [<Erase>]
        member _.cursor
            with set (_: string) = ()
        [<Erase>]
        member _.opacity
            with set (_: string) = ()
        [<Erase>]
        member _.filter
            with set (_: string) = ()
        [<Erase>]
        member _.display
            with set (_: string) = ()
        [<Erase>]
        member _.visibility
            with set (_: string) = ()
        [<Erase>]
        member _.``color-interpolation``
            with set (_: string) = ()
        [<Erase>]
        member _.``color-rendering``
            with set (_: string) = ()
        [<Erase>]
        member _.stroke
            with set (_: string) = ()
        [<Erase>]
        member _.``stroke-dasharray``
            with set (_: string) = ()
        [<Erase>]
        member _.``stroke-dashoffset``
            with set (_: string) = ()
        [<Erase>]
        member _.``stroke-linecap``
            with set (_: string) = ()
        [<Erase>]
        member _.``stroke-linejoin``
            with set (_: string) = ()
        [<Erase>]
        member _.``stroke-miterlimit``
            with set (_: string) = ()
        [<Erase>]
        member _.``stroke-opacity``
            with set (_: string) = ()
        [<Erase>]
        member _.``stroke-width``
            with set (_: string) = ()
        [<Erase>]
        member _.color
            with set (_: string) = ()
        [<Erase>]
        member _.fill
            with set (_: string) = ()
        [<Erase>]
        member _.``fill-opacity``
            with set (_: string) = ()
        [<Erase>]
        member _.``fill-rule``
            with set (_: string) = ()
        [<Erase>]
        member _.``shape-rendering``
            with set (_: string) = ()
        [<Erase>]
        member _.pathLength
            with set (_: string) = ()

    type TextContentElementSVGAttributes with
        [<Erase>]
        member _.``font-family``
            with set (_: string) = ()
        [<Erase>]
        member _.``font-size``
            with set (_: string) = ()
        [<Erase>]
        member _.``font-size-adjust``
            with set (_: string) = ()
        [<Erase>]
        member _.``font-stretch``
            with set (_: string) = ()
        [<Erase>]
        member _.``font-style``
            with set (_: string) = ()
        [<Erase>]
        member _.``font-variant``
            with set (_: string) = ()
        [<Erase>]
        member _.``font-weight``
            with set (_: string) = ()
        [<Erase>]
        member _.kerning
            with set (_: string) = ()
        [<Erase>]
        member _.``letter-spacing``
            with set (_: string) = ()
        [<Erase>]
        member _.``word-spacing``
            with set (_: string) = ()
        [<Erase>]
        member _.``text-decoration``
            with set (_: string) = ()
        [<Erase>]
        member _.``glyph-orientation-horizontal``
            with set (_: string) = ()
        [<Erase>]
        member _.``glyph-orientation-vertical``
            with set (_: string) = ()
        [<Erase>]
        member _.direction
            with set (_: string) = ()
        [<Erase>]
        member _.``unicode-bidi``
            with set (_: string) = ()
        [<Erase>]
        member _.``text-anchor``
            with set (_: string) = ()
        [<Erase>]
        member _.``dominant-baseline``
            with set (_: string) = ()
        [<Erase>]
        member _.color
            with set (_: string) = ()
        [<Erase>]
        member _.fill
            with set (_: string) = ()
        [<Erase>]
        member _.``fill-opacity``
            with set (_: string) = ()
        [<Erase>]
        member _.``fill-rule``
            with set (_: string) = ()
        [<Erase>]
        member _.stroke
            with set (_: string) = ()
        [<Erase>]
        member _.``stroke-dasharray``
            with set (_: string) = ()
        [<Erase>]
        member _.``stroke-dashoffset``
            with set (_: string) = ()
        [<Erase>]
        member _.``stroke-linecap``
            with set (_: string) = ()
        [<Erase>]
        member _.``stroke-linejoin``
            with set (_: string) = ()
        [<Erase>]
        member _.``stroke-miterlimit``
            with set (_: string) = ()
        [<Erase>]
        member _.``stroke-opacity``
            with set (_: string) = ()
        [<Erase>]
        member _.``stroke-width``
            with set (_: string) = ()

    type PresentationSVGAttributes with
        [<Erase>]
        member _.``alignment-baseline``
            with set (_: string) = ()
        [<Erase>]
        member _.``baseline-shift``
            with set (_: string) = ()
        [<Erase>]
        member _.clip
            with set (_: string) = ()
        [<Erase>]
        member _.``clip-path``
            with set (_: string) = ()
        [<Erase>]
        member _.``clip-rule``
            with set (_: string) = ()
        [<Erase>]
        member _.color
            with set (_: string) = ()
        [<Erase>]
        member _.``color-interpolation``
            with set (_: string) = ()
        [<Erase>]
        member _.``color-interpolation-filters``
            with set (_: string) = ()
        [<Erase>]
        member _.``color-profile``
            with set (_: string) = ()
        [<Erase>]
        member _.``color-rendering``
            with set (_: string) = ()
        [<Erase>]
        member _.cursor
            with set (_: string) = ()
        [<Erase>]
        member _.direction
            with set (_: string) = ()
        [<Erase>]
        member _.display
            with set (_: string) = ()
        [<Erase>]
        member _.``dominant-baseline``
            with set (_: string) = ()
        [<Erase>]
        member _.``enable-background``
            with set (_: string) = ()
        [<Erase>]
        member _.fill
            with set (_: string) = ()
        [<Erase>]
        member _.``fill-opacity``
            with set (_: string) = ()
        [<Erase>]
        member _.``fill-rule``
            with set (_: string) = ()
        [<Erase>]
        member _.filter
            with set (_: string) = ()
        [<Erase>]
        member _.``flood-color``
            with set (_: string) = ()
        [<Erase>]
        member _.``flood-opacity``
            with set (_: string) = ()
        [<Erase>]
        member _.``font-family``
            with set (_: string) = ()
        [<Erase>]
        member _.``font-size``
            with set (_: string) = ()
        [<Erase>]
        member _.``font-size-adjust``
            with set (_: string) = ()
        [<Erase>]
        member _.``font-stretch``
            with set (_: string) = ()
        [<Erase>]
        member _.``font-style``
            with set (_: string) = ()
        [<Erase>]
        member _.``font-variant``
            with set (_: string) = ()
        [<Erase>]
        member _.``font-weight``
            with set (_: string) = ()
        [<Erase>]
        member _.``glyph-orientation-horizontal``
            with set (_: string) = ()
        [<Erase>]
        member _.``glyph-orientation-vertical``
            with set (_: string) = ()
        [<Erase>]
        member _.``image-rendering``
            with set (_: string) = ()
        [<Erase>]
        member _.kerning
            with set (_: string) = ()
        [<Erase>]
        member _.``letter-spacing``
            with set (_: string) = ()
        [<Erase>]
        member _.``lighting-color``
            with set (_: string) = ()
        [<Erase>]
        member _.``marker-end``
            with set (_: string) = ()
        [<Erase>]
        member _.``marker-mid``
            with set (_: string) = ()
        [<Erase>]
        member _.``marker-start``
            with set (_: string) = ()
        [<Erase>]
        member _.mask
            with set (_: string) = ()
        [<Erase>]
        member _.opacity
            with set (_: string) = ()
        [<Erase>]
        member _.overflow
            with set (_: string) = ()
        [<Erase>]
        member _.pathLength
            with set (_: string) = ()
        [<Erase>]
        member _.``pointer-events``
            with set (_: string) = ()
        [<Erase>]
        member _.``shape-rendering``
            with set (_: string) = ()
        [<Erase>]
        member _.``stop-color``
            with set (_: string) = ()
        [<Erase>]
        member _.``stop-opacity``
            with set (_: string) = ()
        [<Erase>]
        member _.stroke
            with set (_: string) = ()
        [<Erase>]
        member _.``stroke-dasharray``
            with set (_: string) = ()
        [<Erase>]
        member _.``stroke-dashoffset``
            with set (_: string) = ()
        [<Erase>]
        member _.``stroke-linecap``
            with set (_: string) = ()
        [<Erase>]
        member _.``stroke-linejoin``
            with set (_: string) = ()
        [<Erase>]
        member _.``stroke-miterlimit``
            with set (_: string) = ()
        [<Erase>]
        member _.``stroke-opacity``
            with set (_: string) = ()
        [<Erase>]
        member _.``stroke-width``
            with set (_: string) = ()
        [<Erase>]
        member _.``text-anchor``
            with set (_: string) = ()
        [<Erase>]
        member _.``text-decoration``
            with set (_: string) = ()
        [<Erase>]
        member _.``text-rendering``
            with set (_: string) = ()
        [<Erase>]
        member _.``unicode-bidi``
            with set (_: string) = ()
        [<Erase>]
        member _.visibility
            with set (_: string) = ()
        [<Erase>]
        member _.``word-spacing``
            with set (_: string) = ()
        [<Erase>]
        member _.``writing-mode``
            with set (_: string) = ()

    type FilterPrimitiveElementSVGAttributes with
        [<Erase>]
        member _.x
            with set (_: string) = ()
        [<Erase>]
        member _.y
            with set (_: string) = ()
        [<Erase>]
        member _.width
            with set (_: string) = ()
        [<Erase>]
        member _.height
            with set (_: string) = ()
        [<Erase>]
        member _.result
            with set (_: string) = ()
        [<Erase>]
        member _.``color-interpolation-filters``
            with set (_: string) = ()

    type SingleInputFilterSVGAttributes with
        [<Erase>]
        member _.in'
            with set (_: string) = ()

    type DoubleInputFilterSVGAttributes with
        [<Erase>]
        member _.in'
            with set (_: string) = ()
        [<Erase>]
        member _.in2
            with set (_: string) = ()

    type FitToViewBoxSVGAttributes with
        [<Erase>]
        member _.viewBox
            with set (_: string) = ()
        [<Erase>]
        member _.preserveAspectRatio
            with set (_: string) = ()

    type GradientElementSVGAttributes with
        [<Erase>]
        member _.gradientUnits
            with set (_: string) = ()
        [<Erase>]
        member _.gradientTransform
            with set (_: string) = ()
        [<Erase>]
        member _.spreadMethod
            with set (_: string) = ()
        [<Erase>]
        member _.href
            with set (_: string) = ()

    [<Erase>]
    type animate() =
        interface HtmlTag
        interface HtmlContainer
        interface AnimationElementSVGAttributes
        interface AnimationAttributeTargetSVGAttributes
        interface AnimationTimingSVGAttributes
        interface AnimationValueSVGAttributes
        interface AnimationAdditionSVGAttributes
        interface StylableSVGAttributes
        [<Erase>]
        member _.``color-interpolation``
            with set (_: string) = ()
        [<Erase>]
        member _.``color-rendering``
            with set (_: string) = ()

    [<Erase>]
    type animateMotion() =
        interface HtmlTag
        interface HtmlContainer
        interface AnimationElementSVGAttributes
        interface AnimationTimingSVGAttributes
        interface AnimationValueSVGAttributes
        interface AnimationAdditionSVGAttributes
        [<Erase>]
        member _.path
            with set (_: string) = ()
        [<Erase>]
        member _.keyPoints
            with set (_: string) = ()
        [<Erase>]
        member _.rotate
            with set (_: string) = ()
        [<Erase>]
        member _.origin
            with set (_: string) = ()

    [<Erase>]
    type animateTransform() =
        interface HtmlTag
        interface HtmlContainer
        interface AnimationElementSVGAttributes
        interface AnimationAttributeTargetSVGAttributes
        interface AnimationTimingSVGAttributes
        interface AnimationValueSVGAttributes
        interface AnimationAdditionSVGAttributes
        [<Erase>]
        member _.type'
            with set (_: string) = ()

    [<Erase>]
    type circle() =
        interface HtmlTag
        interface HtmlContainer
        interface GraphicsElementSVGAttributes
        interface ShapeElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface StylableSVGAttributes
        interface TransformableSVGAttributes
        [<Erase>]
        member _.cx
            with set (_: string) = ()
        [<Erase>]
        member _.cy
            with set (_: string) = ()
        [<Erase>]
        member _.r
            with set (_: string) = ()

    [<Erase>]
    type clipPath() =
        interface HtmlTag
        interface HtmlContainer
        interface CoreSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface ExternalResourceSVGAttributes
        interface StylableSVGAttributes
        interface TransformableSVGAttributes
        [<Erase>]
        member _.clipPathUnits
            with set (_: string) = ()
        [<Erase>]
        member _.``clip-path``
            with set (_: string) = ()

    [<Erase>]
    type defs() =
        interface HtmlTag
        interface HtmlContainer
        interface ContainerElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface ExternalResourceSVGAttributes
        interface StylableSVGAttributes
        interface TransformableSVGAttributes

    [<Erase>]
    type desc() =
        interface HtmlTag
        interface HtmlContainer
        interface CoreSVGAttributes
        interface StylableSVGAttributes

    [<Erase>]
    type ellipse() =
        interface HtmlTag
        interface HtmlContainer
        interface GraphicsElementSVGAttributes
        interface ShapeElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface ExternalResourceSVGAttributes
        interface StylableSVGAttributes
        interface TransformableSVGAttributes
        [<Erase>]
        member _.cx
            with set (_: string) = ()
        [<Erase>]
        member _.cy
            with set (_: string) = ()
        [<Erase>]
        member _.rx
            with set (_: string) = ()
        [<Erase>]
        member _.ry
            with set (_: string) = ()

    [<Erase>]
    type feBlend() =
        interface HtmlTag
        interface HtmlContainer
        interface FilterPrimitiveElementSVGAttributes
        interface DoubleInputFilterSVGAttributes
        interface StylableSVGAttributes
        [<Erase>]
        member _.mode
            with set (_: string) = ()

    [<Erase>]
    type feColorMatrix() =
        interface HtmlTag
        interface HtmlContainer
        interface FilterPrimitiveElementSVGAttributes
        interface SingleInputFilterSVGAttributes
        interface StylableSVGAttributes
        [<Erase>]
        member _.type'
            with set (_: string) = ()
        [<Erase>]
        member _.values
            with set (_: string) = ()

    [<Erase>]
    type feComponentTransfer() =
        interface HtmlTag
        interface HtmlContainer
        interface FilterPrimitiveElementSVGAttributes
        interface SingleInputFilterSVGAttributes
        interface StylableSVGAttributes

    [<Erase>]
    type feComposite() =
        interface HtmlTag
        interface HtmlContainer
        interface FilterPrimitiveElementSVGAttributes
        interface DoubleInputFilterSVGAttributes
        interface StylableSVGAttributes
        [<Erase>]
        member _.operator
            with set (_: string) = ()
        [<Erase>]
        member _.k1
            with set (_: string) = ()
        [<Erase>]
        member _.k2
            with set (_: string) = ()
        [<Erase>]
        member _.k3
            with set (_: string) = ()
        [<Erase>]
        member _.k4
            with set (_: string) = ()

    [<Erase>]
    type feConvolveMatrix() =
        interface HtmlTag
        interface HtmlContainer
        interface FilterPrimitiveElementSVGAttributes
        interface SingleInputFilterSVGAttributes
        interface StylableSVGAttributes
        [<Erase>]
        member _.order
            with set (_: string) = ()
        [<Erase>]
        member _.kernelMatrix
            with set (_: string) = ()
        [<Erase>]
        member _.divisor
            with set (_: string) = ()
        [<Erase>]
        member _.bias
            with set (_: string) = ()
        [<Erase>]
        member _.targetX
            with set (_: string) = ()
        [<Erase>]
        member _.targetY
            with set (_: string) = ()
        [<Erase>]
        member _.edgeMode
            with set (_: string) = ()
        [<Erase>]
        member _.kernelUnitLength
            with set (_: string) = ()
        [<Erase>]
        member _.preserveAlpha
            with set (_: string) = ()

    [<Erase>]
    type feDiffuseLighting() =
        interface HtmlTag
        interface HtmlContainer
        interface FilterPrimitiveElementSVGAttributes
        interface SingleInputFilterSVGAttributes
        interface StylableSVGAttributes
        [<Erase>]
        member _.``lighting-color``
            with set (_: string) = ()
        [<Erase>]
        member _.color
            with set (_: string) = ()
        [<Erase>]
        member _.surfaceScale
            with set (_: string) = ()
        [<Erase>]
        member _.diffuseConstant
            with set (_: string) = ()
        [<Erase>]
        member _.kernelUnitLength
            with set (_: string) = ()

    [<Erase>]
    type feDisplacementMap() =
        interface HtmlTag
        interface HtmlContainer
        interface FilterPrimitiveElementSVGAttributes
        interface DoubleInputFilterSVGAttributes
        interface StylableSVGAttributes
        [<Erase>]
        member _.scale
            with set (_: string) = ()
        [<Erase>]
        member _.xChannelSelector
            with set (_: string) = ()
        [<Erase>]
        member _.yChannelSelector
            with set (_: string) = ()

    [<Erase>]
    type feDistantLight() =
        interface HtmlTag
        interface HtmlContainer
        interface LightSourceElementSVGAttributes
        [<Erase>]
        member _.azimuth
            with set (_: string) = ()
        [<Erase>]
        member _.elevation
            with set (_: string) = ()

    [<Erase>]
    type feDropShadow() =
        interface HtmlTag
        interface HtmlContainer
        interface CoreSVGAttributes
        interface FilterPrimitiveElementSVGAttributes
        interface StylableSVGAttributes
        [<Erase>]
        member _.color
            with set (_: string) = ()
        [<Erase>]
        member _.``flood-color``
            with set (_: string) = ()
        [<Erase>]
        member _.``flood-opacity``
            with set (_: string) = ()
        [<Erase>]
        member _.dx
            with set (_: string) = ()
        [<Erase>]
        member _.dy
            with set (_: string) = ()
        [<Erase>]
        member _.stdDeviation
            with set (_: string) = ()

    [<Erase>]
    type feFlood() =
        interface HtmlTag
        interface HtmlContainer
        interface FilterPrimitiveElementSVGAttributes
        interface StylableSVGAttributes
        [<Erase>]
        member _.color
            with set (_: string) = ()
        [<Erase>]
        member _.``flood-color``
            with set (_: string) = ()
        [<Erase>]
        member _.``flood-opacity``
            with set (_: string) = ()

    [<Erase>]
    type feFuncA() =
        interface HtmlTag
        interface HtmlContainer
        interface CoreSVGAttributes
        [<Erase>]
        member _.type'
            with set (_: string) = ()
        [<Erase>]
        member _.tableValues
            with set (_: string) = ()
        [<Erase>]
        member _.slope
            with set (_: string) = ()
        [<Erase>]
        member _.intercept
            with set (_: string) = ()
        [<Erase>]
        member _.amplitude
            with set (_: string) = ()
        [<Erase>]
        member _.exponent
            with set (_: string) = ()
        [<Erase>]
        member _.offset
            with set (_: string) = ()

    [<Erase>]
    type feFuncB() =
        interface HtmlTag
        interface HtmlContainer
        interface CoreSVGAttributes
        [<Erase>]
        member _.type'
            with set (_: string) = ()
        [<Erase>]
        member _.tableValues
            with set (_: string) = ()
        [<Erase>]
        member _.slope
            with set (_: string) = ()
        [<Erase>]
        member _.intercept
            with set (_: string) = ()
        [<Erase>]
        member _.amplitude
            with set (_: string) = ()
        [<Erase>]
        member _.exponent
            with set (_: string) = ()
        [<Erase>]
        member _.offset
            with set (_: string) = ()

    [<Erase>]
    type feFuncG() =
        interface HtmlTag
        interface HtmlContainer
        interface CoreSVGAttributes
        [<Erase>]
        member _.type'
            with set (_: string) = ()
        [<Erase>]
        member _.tableValues
            with set (_: string) = ()
        [<Erase>]
        member _.slope
            with set (_: string) = ()
        [<Erase>]
        member _.intercept
            with set (_: string) = ()
        [<Erase>]
        member _.amplitude
            with set (_: string) = ()
        [<Erase>]
        member _.exponent
            with set (_: string) = ()
        [<Erase>]
        member _.offset
            with set (_: string) = ()

    [<Erase>]
    type feFuncR() =
        interface HtmlTag
        interface HtmlContainer
        interface CoreSVGAttributes
        [<Erase>]
        member _.type'
            with set (_: string) = ()
        [<Erase>]
        member _.tableValues
            with set (_: string) = ()
        [<Erase>]
        member _.slope
            with set (_: string) = ()
        [<Erase>]
        member _.intercept
            with set (_: string) = ()
        [<Erase>]
        member _.amplitude
            with set (_: string) = ()
        [<Erase>]
        member _.exponent
            with set (_: string) = ()
        [<Erase>]
        member _.offset
            with set (_: string) = ()

    [<Erase>]
    type feGaussianBlur() =
        interface HtmlTag
        interface HtmlContainer
        interface FilterPrimitiveElementSVGAttributes
        interface SingleInputFilterSVGAttributes
        interface StylableSVGAttributes
        [<Erase>]
        member _.stdDeviation
            with set (_: string) = ()

    [<Erase>]
    type feImage() =
        interface HtmlTag
        interface HtmlContainer
        interface FilterPrimitiveElementSVGAttributes
        interface ExternalResourceSVGAttributes
        interface StylableSVGAttributes
        [<Erase>]
        member _.preserveAspectRatio
            with set (_: string) = ()
        [<Erase>]
        member _.href
            with set (_: string) = ()

    [<Erase>]
    type feMerge() =
        interface HtmlTag
        interface HtmlContainer
        interface FilterPrimitiveElementSVGAttributes
        interface StylableSVGAttributes

    [<Erase>]
    type feMergeNode() =
        interface HtmlTag
        interface HtmlContainer
        interface CoreSVGAttributes
        interface SingleInputFilterSVGAttributes

    [<Erase>]
    type feMorphology() =
        interface HtmlTag
        interface HtmlContainer
        interface FilterPrimitiveElementSVGAttributes
        interface SingleInputFilterSVGAttributes
        interface StylableSVGAttributes
        [<Erase>]
        member _.operator
            with set (_: string) = ()
        [<Erase>]
        member _.radius
            with set (_: string) = ()

    [<Erase>]
    type feOffset() =
        interface HtmlTag
        interface HtmlContainer
        interface FilterPrimitiveElementSVGAttributes
        interface SingleInputFilterSVGAttributes
        interface StylableSVGAttributes
        [<Erase>]
        member _.dx
            with set (_: string) = ()
        [<Erase>]
        member _.dy
            with set (_: string) = ()

    [<Erase>]
    type fePointLight() =
        interface HtmlTag
        interface HtmlContainer
        interface LightSourceElementSVGAttributes
        [<Erase>]
        member _.x
            with set (_: string) = ()
        [<Erase>]
        member _.y
            with set (_: string) = ()
        [<Erase>]
        member _.z
            with set (_: string) = ()

    [<Erase>]
    type feSpecularLighting() =
        interface HtmlTag
        interface HtmlContainer
        interface FilterPrimitiveElementSVGAttributes
        interface SingleInputFilterSVGAttributes
        interface StylableSVGAttributes
        [<Erase>]
        member _.``lighting-color``
            with set (_: string) = ()
        [<Erase>]
        member _.color
            with set (_: string) = ()
        [<Erase>]
        member _.surfaceScale
            with set (_: string) = ()
        [<Erase>]
        member _.specularConstant
            with set (_: string) = ()
        [<Erase>]
        member _.specularExponent
            with set (_: string) = ()
        [<Erase>]
        member _.kernelUnitLength
            with set (_: string) = ()

    [<Erase>]
    type feSpotLight() =
        interface HtmlTag
        interface HtmlContainer
        interface LightSourceElementSVGAttributes
        [<Erase>]
        member _.x
            with set (_: string) = ()
        [<Erase>]
        member _.y
            with set (_: string) = ()
        [<Erase>]
        member _.z
            with set (_: string) = ()
        [<Erase>]
        member _.pointsAtX
            with set (_: string) = ()
        [<Erase>]
        member _.pointsAtY
            with set (_: string) = ()
        [<Erase>]
        member _.pointsAtZ
            with set (_: string) = ()
        [<Erase>]
        member _.specularExponent
            with set (_: string) = ()
        [<Erase>]
        member _.limitingConeAngle
            with set (_: string) = ()

    [<Erase>]
    type feTile() =
        interface HtmlTag
        interface HtmlContainer
        interface FilterPrimitiveElementSVGAttributes
        interface SingleInputFilterSVGAttributes
        interface StylableSVGAttributes

    [<Erase>]
    type feTurbulence() =
        interface HtmlTag
        interface HtmlContainer
        interface FilterPrimitiveElementSVGAttributes
        interface StylableSVGAttributes
        [<Erase>]
        member _.baseFrequency
            with set (_: string) = ()
        [<Erase>]
        member _.numOctaves
            with set (_: string) = ()
        [<Erase>]
        member _.seed
            with set (_: string) = ()
        [<Erase>]
        member _.stitchTiles
            with set (_: string) = ()
        [<Erase>]
        member _.type'
            with set (_: string) = ()

    [<Erase>]
    type filter() =
        interface HtmlTag
        interface HtmlContainer
        interface CoreSVGAttributes
        interface ExternalResourceSVGAttributes
        interface StylableSVGAttributes
        [<Erase>]
        member _.filterUnits
            with set (_: string) = ()
        [<Erase>]
        member _.primitiveUnits
            with set (_: string) = ()
        [<Erase>]
        member _.x
            with set (_: string) = ()
        [<Erase>]
        member _.y
            with set (_: string) = ()
        [<Erase>]
        member _.width
            with set (_: string) = ()
        [<Erase>]
        member _.height
            with set (_: string) = ()
        [<Erase>]
        member _.filterRes
            with set (_: string) = ()

    [<Erase>]
    type foreignObject() =
        interface HtmlTag
        interface HtmlContainer
        interface NewViewportSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface ExternalResourceSVGAttributes
        interface StylableSVGAttributes
        interface TransformableSVGAttributes
        [<Erase>]
        member _.visibility
            with set (_: string) = ()
        [<Erase>]
        member _.display
            with set (_: string) = ()
        [<Erase>]
        member _.x
            with set (_: string) = ()
        [<Erase>]
        member _.y
            with set (_: string) = ()
        [<Erase>]
        member _.width
            with set (_: string) = ()
        [<Erase>]
        member _.height
            with set (_: string) = ()

    [<Erase>]
    type g() =
        interface HtmlTag
        interface HtmlContainer
        interface ContainerElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface ExternalResourceSVGAttributes
        interface StylableSVGAttributes
        interface TransformableSVGAttributes
        [<Erase>]
        member _.visibility
            with set (_: string) = ()
        [<Erase>]
        member _.display
            with set (_: string) = ()

    [<Erase>]
    type image() =
        interface HtmlTag
        interface HtmlContainer
        interface NewViewportSVGAttributes
        interface GraphicsElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface StylableSVGAttributes
        interface TransformableSVGAttributes
        [<Erase>]
        member _.``image-rendering``
            with set (_: string) = ()
        [<Erase>]
        member _.``color-profile``
            with set (_: string) = ()
        [<Erase>]
        member _.x
            with set (_: string) = ()
        [<Erase>]
        member _.y
            with set (_: string) = ()
        [<Erase>]
        member _.width
            with set (_: string) = ()
        [<Erase>]
        member _.height
            with set (_: string) = ()
        [<Erase>]
        member _.preserveAspectRatio
            with set (_: string) = ()
        [<Erase>]
        member _.href
            with set (_: string) = ()

    [<Erase>]
    type line() =
        interface HtmlTag
        interface HtmlContainer
        interface GraphicsElementSVGAttributes
        interface ShapeElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface ExternalResourceSVGAttributes
        interface StylableSVGAttributes
        interface TransformableSVGAttributes
        [<Erase>]
        member _.``marker-end``
            with set (_: string) = ()
        [<Erase>]
        member _.``marker-mid``
            with set (_: string) = ()
        [<Erase>]
        member _.``marker-start``
            with set (_: string) = ()
        [<Erase>]
        member _.x1
            with set (_: string) = ()
        [<Erase>]
        member _.y1
            with set (_: string) = ()
        [<Erase>]
        member _.x2
            with set (_: string) = ()
        [<Erase>]
        member _.y2
            with set (_: string) = ()

    [<Erase>]
    type linearGradient() =
        interface HtmlTag
        interface HtmlContainer
        interface GradientElementSVGAttributes
        [<Erase>]
        member _.x1
            with set (_: string) = ()
        [<Erase>]
        member _.x2
            with set (_: string) = ()
        [<Erase>]
        member _.y1
            with set (_: string) = ()
        [<Erase>]
        member _.y2
            with set (_: string) = ()

    [<Erase>]
    type marker() =
        interface HtmlTag
        interface HtmlContainer
        interface ContainerElementSVGAttributes
        interface ExternalResourceSVGAttributes
        interface StylableSVGAttributes
        interface FitToViewBoxSVGAttributes
        [<Erase>]
        member _.clip
            with set (_: string) = ()
        [<Erase>]
        member _.overflow
            with set (_: string) = ()
        [<Erase>]
        member _.markerUnits
            with set (_: string) = ()
        [<Erase>]
        member _.refX
            with set (_: string) = ()
        [<Erase>]
        member _.refY
            with set (_: string) = ()
        [<Erase>]
        member _.markerWidth
            with set (_: string) = ()
        [<Erase>]
        member _.markerHeight
            with set (_: string) = ()
        [<Erase>]
        member _.orient
            with set (_: string) = ()

    [<Erase>]
    type mask() =
        interface HtmlTag
        interface HtmlContainer
        interface ConditionalProcessingSVGAttributes
        interface ExternalResourceSVGAttributes
        interface StylableSVGAttributes
        [<Erase>]
        member _.filter
            with set (_: string) = ()
        [<Erase>]
        member _.opacity
            with set (_: string) = ()
        [<Erase>]
        member _.maskUnits
            with set (_: string) = ()
        [<Erase>]
        member _.maskContentUnits
            with set (_: string) = ()
        [<Erase>]
        member _.x
            with set (_: string) = ()
        [<Erase>]
        member _.y
            with set (_: string) = ()
        [<Erase>]
        member _.width
            with set (_: string) = ()
        [<Erase>]
        member _.height
            with set (_: string) = ()

    [<Erase>]
    type metadata() =
        interface HtmlTag
        interface HtmlContainer
        interface CoreSVGAttributes

    [<Erase>]
    type mpath() =
        interface HtmlTag
        interface HtmlContainer
        interface CoreSVGAttributes

    [<Erase>]
    type path() =
        interface HtmlTag
        interface HtmlContainer
        interface GraphicsElementSVGAttributes
        interface ShapeElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface ExternalResourceSVGAttributes
        interface StylableSVGAttributes
        interface TransformableSVGAttributes
        [<Erase>]
        member _.``marker-end``
            with set (_: string) = ()
        [<Erase>]
        member _.``marker-mid``
            with set (_: string) = ()
        [<Erase>]
        member _.``marker-start``
            with set (_: string) = ()
        [<Erase>]
        member _.d
            with set (_: string) = ()
        [<Erase>]
        member _.pathLength
            with set (_: string) = ()

    [<Erase>]
    type pattern() =
        interface HtmlTag
        interface HtmlContainer
        interface ContainerElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface ExternalResourceSVGAttributes
        interface StylableSVGAttributes
        interface FitToViewBoxSVGAttributes
        [<Erase>]
        member _.x
            with set (_: string) = ()
        [<Erase>]
        member _.y
            with set (_: string) = ()
        [<Erase>]
        member _.width
            with set (_: string) = ()
        [<Erase>]
        member _.height
            with set (_: string) = ()
        [<Erase>]
        member _.patternUnits
            with set (_: string) = ()
        [<Erase>]
        member _.patternContentUnits
            with set (_: string) = ()
        [<Erase>]
        member _.patternTransform
            with set (_: string) = ()
        [<Erase>]
        member _.href
            with set (_: string) = ()
        [<Erase>]
        member _.clip
            with set (_: string) = ()
        [<Erase>]
        member _.overflow
            with set (_: string) = ()

    [<Erase>]
    type polygon() =
        interface HtmlTag
        interface HtmlContainer
        interface GraphicsElementSVGAttributes
        interface ShapeElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface ExternalResourceSVGAttributes
        interface StylableSVGAttributes
        interface TransformableSVGAttributes
        [<Erase>]
        member _.``marker-end``
            with set (_: string) = ()
        [<Erase>]
        member _.``marker-mid``
            with set (_: string) = ()
        [<Erase>]
        member _.``marker-start``
            with set (_: string) = ()
        [<Erase>]
        member _.points
            with set (_: string) = ()

    [<Erase>]
    type polyline() =
        interface HtmlTag
        interface HtmlContainer
        interface GraphicsElementSVGAttributes
        interface ShapeElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface ExternalResourceSVGAttributes
        interface StylableSVGAttributes
        interface TransformableSVGAttributes
        [<Erase>]
        member _.``marker-end``
            with set (_: string) = ()
        [<Erase>]
        member _.``marker-mid``
            with set (_: string) = ()
        [<Erase>]
        member _.``marker-start``
            with set (_: string) = ()
        [<Erase>]
        member _.points
            with set (_: string) = ()

    [<Erase>]
    type radialGradient() =
        interface HtmlTag
        interface HtmlContainer
        interface GradientElementSVGAttributes
        [<Erase>]
        member _.cx
            with set (_: string) = ()
        [<Erase>]
        member _.cy
            with set (_: string) = ()
        [<Erase>]
        member _.r
            with set (_: string) = ()
        [<Erase>]
        member _.fx
            with set (_: string) = ()
        [<Erase>]
        member _.fy
            with set (_: string) = ()

    [<Erase>]
    type rect() =
        interface HtmlTag
        interface HtmlContainer
        interface GraphicsElementSVGAttributes
        interface ShapeElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface ExternalResourceSVGAttributes
        interface StylableSVGAttributes
        interface TransformableSVGAttributes
        [<Erase>]
        member _.x
            with set (_: string) = ()
        [<Erase>]
        member _.y
            with set (_: string) = ()
        [<Erase>]
        member _.width
            with set (_: string) = ()
        [<Erase>]
        member _.height
            with set (_: string) = ()
        [<Erase>]
        member _.rx
            with set (_: string) = ()
        [<Erase>]
        member _.ry
            with set (_: string) = ()

    [<Erase>]
    type set() =
        interface HtmlTag
        interface HtmlContainer
        interface CoreSVGAttributes
        interface StylableSVGAttributes
        interface AnimationTimingSVGAttributes

    [<Erase>]
    type stop() =
        interface HtmlTag
        interface HtmlContainer
        interface CoreSVGAttributes
        interface StylableSVGAttributes
        [<Erase>]
        member _.color
            with set (_: string) = ()
        [<Erase>]
        member _.``stop-color``
            with set (_: string) = ()
        [<Erase>]
        member _.``stop-opacity``
            with set (_: string) = ()
        [<Erase>]
        member _.offset
            with set (_: string) = ()

    [<Erase>]
    type svg() =
        interface HtmlTag
        interface HtmlContainer
        interface ContainerElementSVGAttributes
        interface NewViewportSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface ExternalResourceSVGAttributes
        interface StylableSVGAttributes
        interface FitToViewBoxSVGAttributes
        interface ZoomAndPanSVGAttributes
        interface PresentationSVGAttributes
        [<Erase>]
        member _.version
            with set (_: string) = ()
        [<Erase>]
        member _.baseProfile
            with set (_: string) = ()
        [<Erase>]
        member _.x
            with set (_: string) = ()
        [<Erase>]
        member _.y
            with set (_: string) = ()
        [<Erase>]
        member _.width
            with set (_: string) = ()
        [<Erase>]
        member _.height
            with set (_: string) = ()
        [<Erase>]
        member _.contentScriptType
            with set (_: string) = ()
        [<Erase>]
        member _.contentStyleType
            with set (_: string) = ()
        [<Erase>]
        member _.xmlns
            with set (_: string) = ()

    [<Erase>]
    type switch() =
        interface HtmlTag
        interface HtmlContainer
        interface ContainerElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface ExternalResourceSVGAttributes
        interface StylableSVGAttributes
        interface TransformableSVGAttributes
        [<Erase>]
        member _.display
            with set (_: string) = ()
        [<Erase>]
        member _.visibility
            with set (_: string) = ()

    [<Erase>]
    type symbol() =
        interface HtmlTag
        interface HtmlContainer
        interface ContainerElementSVGAttributes
        interface NewViewportSVGAttributes
        interface ExternalResourceSVGAttributes
        interface StylableSVGAttributes
        interface FitToViewBoxSVGAttributes
        [<Erase>]
        member _.width
            with set (_: string) = ()
        [<Erase>]
        member _.height
            with set (_: string) = ()
        [<Erase>]
        member _.preserveAspectRatio
            with set (_: string) = ()
        [<Erase>]
        member _.refX
            with set (_: string) = ()
        [<Erase>]
        member _.refY
            with set (_: string) = ()
        [<Erase>]
        member _.viewBox
            with set (_: string) = ()
        [<Erase>]
        member _.x
            with set (_: string) = ()
        [<Erase>]
        member _.y
            with set (_: string) = ()

    [<Erase>]
    type text() =
        interface HtmlTag
        interface HtmlContainer
        interface TextContentElementSVGAttributes
        interface GraphicsElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface ExternalResourceSVGAttributes
        interface StylableSVGAttributes
        interface TransformableSVGAttributes
        [<Erase>]
        member _.x
            with set (_: string) = ()
        [<Erase>]
        member _.y
            with set (_: string) = ()
        [<Erase>]
        member _.dx
            with set (_: string) = ()
        [<Erase>]
        member _.dy
            with set (_: string) = ()
        [<Erase>]
        member _.rotate
            with set (_: string) = ()
        [<Erase>]
        member _.textLength
            with set (_: string) = ()
        [<Erase>]
        member _.lengthAdjust
            with set (_: string) = ()
        [<Erase>]
        member _.``writing-mode``
            with set (_: string) = ()
        [<Erase>]
        member _.``text-rendering``
            with set (_: string) = ()

    [<Erase>]
    type textPath() =
        interface HtmlTag
        interface HtmlContainer
        interface TextContentElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface ExternalResourceSVGAttributes
        interface StylableSVGAttributes
        [<Erase>]
        member _.startOffset
            with set (_: string) = ()
        [<Erase>]
        member _.method
            with set (_: string) = ()
        [<Erase>]
        member _.spacing
            with set (_: string) = ()
        [<Erase>]
        member _.``alignment-baseline``
            with set (_: string) = ()
        [<Erase>]
        member _.``baseline-shift``
            with set (_: string) = ()
        [<Erase>]
        member _.display
            with set (_: string) = ()
        [<Erase>]
        member _.visibility
            with set (_: string) = ()
        [<Erase>]
        member _.href
            with set (_: string) = ()

    [<Erase>]
    type tspan() =
        interface HtmlTag
        interface HtmlContainer
        interface TextContentElementSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface ExternalResourceSVGAttributes
        interface StylableSVGAttributes
        [<Erase>]
        member _.x
            with set (_: string) = ()
        [<Erase>]
        member _.y
            with set (_: string) = ()
        [<Erase>]
        member _.dx
            with set (_: string) = ()
        [<Erase>]
        member _.dy
            with set (_: string) = ()
        [<Erase>]
        member _.rotate
            with set (_: string) = ()
        [<Erase>]
        member _.textLength
            with set (_: string) = ()
        [<Erase>]
        member _.lengthAdjust
            with set (_: string) = ()
        [<Erase>]
        member _.``alignment-baseline``
            with set (_: string) = ()
        [<Erase>]
        member _.``baseline-shift``
            with set (_: string) = ()
        [<Erase>]
        member _.display
            with set (_: string) = ()
        [<Erase>]
        member _.visibility
            with set (_: string) = ()

    [<Erase>]
    type use'() =
        interface HtmlTag
        interface HtmlContainer
        interface CoreSVGAttributes
        interface StylableSVGAttributes
        interface ConditionalProcessingSVGAttributes
        interface GraphicsElementSVGAttributes
        interface PresentationSVGAttributes
        interface ExternalResourceSVGAttributes
        interface TransformableSVGAttributes
        [<Erase>]
        member _.x
            with set (_: string) = ()
        [<Erase>]
        member _.y
            with set (_: string) = ()
        [<Erase>]
        member _.width
            with set (_: string) = ()
        [<Erase>]
        member _.height
            with set (_: string) = ()
        [<Erase>]
        member _.href
            with set (_: string) = ()

    [<Erase>]
    type view() =
        interface HtmlTag
        interface HtmlContainer
        interface CoreSVGAttributes
        interface ExternalResourceSVGAttributes
        interface FitToViewBoxSVGAttributes
        interface ZoomAndPanSVGAttributes
        [<Erase>]
        member _.viewTarget
            with set (_: string) = ()
