namespace Oxpecker.Solid

open Fable.Core

module Aria =

    type HtmlTag with
        // aria role
        [<Erase>]
        member this.role
            with set (_: string) = ()
        // aria attributes
        [<Erase>]
        member this.ariaActiveDescendant
            with set (_: string) = ()
        [<Erase>]
        member this.ariaAtomic
            with set (_: bool) = ()
        [<Erase>]
        member this.ariaAutoComplete
            with set (_: string) = ()
        [<Erase>]
        member this.ariaBrailleLabel
            with set (_: string) = ()
        [<Erase>]
        member this.ariaBrailleRoleDescription
            with set (_: string) = ()
        [<Erase>]
        member this.ariaBusy
            with set (_: bool) = ()
        [<Erase>]
        member this.ariaChecked
            with set (_: string) = ()
        [<Erase>]
        member this.ariaColCount
            with set (_: int) = ()
        [<Erase>]
        member this.ariaColIndex
            with set (_: int) = ()
        [<Erase>]
        member this.ariaColIndexText
            with set (_: string) = ()
        [<Erase>]
        member this.ariaControls
            with set (_: string) = ()
        [<Erase>]
        member this.ariaCurrent
            with set (_: string) = ()
        [<Erase>]
        member this.ariaDescribedBy
            with set (_: string) = ()
        [<Erase>]
        member this.ariaDescription
            with set (_: string) = ()
        [<Erase>]
        member this.ariaDetails
            with set (_: string) = ()
        [<Erase>]
        member this.ariaDisabled
            with set (_: bool) = ()
        [<Erase>]
        member this.ariaErrorMessage
            with set (_: string) = ()
        [<Erase>]
        member this.ariaExpanded
            with set (_: bool) = ()
        [<Erase>]
        member this.ariaFlowTo
            with set (_: string) = ()
        [<Erase>]
        member this.ariaHasPopup
            with set (_: string) = ()
        [<Erase>]
        member this.ariaHidden
            with set (_: bool) = ()
        [<Erase>]
        member this.ariaInvalid
            with set (_: string) = ()
        [<Erase>]
        member this.ariaKeyShortcuts
            with set (_: string) = ()
        [<Erase>]
        member this.ariaLabel
            with set (_: string) = ()
        [<Erase>]
        member this.ariaLabelledBy
            with set (_: string) = ()
        [<Erase>]
        member this.ariaLevel
            with set (_: int) = ()
        [<Erase>]
        member this.ariaLive
            with set (_: string) = ()
        [<Erase>]
        member this.ariaModal
            with set (_: bool) = ()
        [<Erase>]
        member this.ariaMultiLine
            with set (_: bool) = ()
        [<Erase>]
        member this.ariaMultiSelectable
            with set (_: bool) = ()
        [<Erase>]
        member this.ariaOrientation
            with set (_: string) = ()
        [<Erase>]
        member this.ariaOwns
            with set (_: string) = ()
        [<Erase>]
        member this.ariaPlaceholder
            with set (_: string) = ()
        [<Erase>]
        member this.ariaPosInSet
            with set (_: int) = ()
        [<Erase>]
        member this.ariaPressed
            with set (_: string) = ()
        [<Erase>]
        member this.ariaReadOnly
            with set (_: string) = ()
        [<Erase>]
        member this.ariaRelevant
            with set (_: string) = ()
        [<Erase>]
        member this.ariaRequired
            with set (_: bool) = ()
        [<Erase>]
        member this.ariaRoleDescription
            with set (_: string) = ()
        [<Erase>]
        member this.ariaRowCount
            with set (_: int) = ()
        [<Erase>]
        member this.ariaRowIndex
            with set (_: int) = ()
        [<Erase>]
        member this.ariaRowIndexText
            with set (_: string) = ()
        [<Erase>]
        member this.ariaRowSpan
            with set (_: int) = ()
        [<Erase>]
        member this.ariaSelected
            with set (_: bool) = ()
        [<Erase>]
        member this.ariaSetSize
            with set (_: int) = ()
        [<Erase>]
        member this.ariaSort
            with set (_: string) = ()
        [<Erase>]
        member this.ariaValueMax
            with set (_: string) = ()
        [<Erase>]
        member this.ariaValueMin
            with set (_: string) = ()
        [<Erase>]
        member this.ariaValueNow
            with set (_: string) = ()
        [<Erase>]
        member this.ariaValueText
            with set (_: string) = ()
