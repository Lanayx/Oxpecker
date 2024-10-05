namespace Oxpecker.Solid

module Aria =

    type HtmlTag with
        // aria role
        member this.role with set (value: string) = ()
        // aria attributes
        member this.ariaActiveDescendant with set (value: string) = ()
        member this.ariaAtomic with set (value: bool) = () //TODO
        member this.ariaAutoComplete with set (value: string) = ()
        member this.ariaBrailleLabel with set (value: string) = ()
        member this.ariaBrailleRoleDescription with set (value: string) = ()
        member this.ariaBusy with set (value: bool) = () //TODO
        member this.ariaChecked with set (value: string) = ()
        member this.ariaColCount with set (value: int) = ()
        member this.ariaColIndex with set (value: int) = ()
        member this.ariaColIndexText with set (value: string) = ()
        member this.ariaControls with set (value: string) = ()
        member this.ariaCurrent with set (value: string) = ()
        member this.ariaDescribedBy with set (value: string) = ()
        member this.ariaDescription with set (value: string) = ()
        member this.ariaDetails with set (value: string) = ()
        member this.ariaDisabled with set (value: bool) = () //TODO
        member this.ariaErrorMessage with set (value: string) = ()
        member this.ariaExpanded with set (value: bool) = () //TODO
        member this.ariaFlowTo with set (value: string) = ()
        member this.ariaHasPopup with set (value: string) = ()
        member this.ariaHidden with set (value: bool) = () //TODO
        member this.ariaInvalid with set (value: string) = ()
        member this.ariaKeyShortcuts with set (value: string) = ()
        member this.ariaLabel with set (value: string) = ()
        member this.ariaLabelledBy with set (value: string) = ()
        member this.ariaLevel with set (value: int) = ()
        member this.ariaLive with set (value: string) = ()
        member this.ariaModal with set (bool: bool) = () //TODO
        member this.ariaMultiLine with set (bool: bool) = () //TODO
        member this.ariaMultiSelectable with set (bool: bool) = () //TODO
        member this.ariaOrientation with set (value: string) = ()
        member this.ariaOwns with set (value: string) = ()
        member this.ariaPlaceholder with set (value: string) = ()
        member this.ariaPosInSet with set (value: int) = ()
        member this.ariaPressed with set (value: string) = ()
        member this.ariaReadOnly with set (value: string) = ()
        member this.ariaRelevant with set (value: string) = ()
        member this.ariaRequired with set (bool: bool) = () //TODO
        member this.ariaRoleDescription with set (value: string) = ()
        member this.ariaRowCount with set (value: int) = ()
        member this.ariaRowIndex with set (value: int) = ()
        member this.ariaRowIndexText with set (value: string) = ()
        member this.ariaRowSpan with set (value: int) = ()
        member this.ariaSelected with set (bool: bool) = () //TODO
        member this.ariaSetSize with set (value: int) = ()
        member this.ariaSort with set (value: string) = ()
        member this.ariaValueMax with set (value: string) = ()
        member this.ariaValueMin with set (value: string) = ()
        member this.ariaValueNow with set (value: string) = ()
        member this.ariaValueText with set (value: string) = ()
