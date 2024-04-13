using System.Linq;
using RimWorld;
using Verse;

namespace HaulToBuilding
{
    public static class Utils
    {
        public static Zone_Stockpile FakeStockpile(this ISlotGroupParent parent)
        {
            var pile = new Zone_Stockpile {settings = parent.GetStoreSettings()};
            return pile;
        }

        public static string TakeFromText(this ExtraBillData extraData) => !extraData.TakeFrom.Any()
            ? "HaulToBuilding.TakeFromAll".Translate()
            : "HaulToBuilding.TakeFromSpecific".Translate(
                extraData.TakeFrom.Count == 1
                    ? extraData.TakeFrom.First().SlotYielderLabel()
                    : "HaulToBuilding.Places".Translate(extraData.TakeFrom.Count).ToString());

        public static bool ValidTakeFrom(this ISlotGroupParent parent, Bill bill) => bill.recipe.ingredients.Any(ing =>
            ing.IsFixedIngredient
                ? parent.GetStoreSettings().AllowedToAccept(ing.FixedIngredient)
                : ing.filter.AllowedThingDefs.Any(def => parent.GetStoreSettings().AllowedToAccept(def)));

        public static string LookInText(this Bill_Production bill, ExtraBillData extraData) => bill.includeGroup == null && extraData.LookInStorage == null
            ? "IncludeFromAll".Translate()
            : "IncludeSpecific".Translate(
                ((ISlotGroupParent) bill.includeGroup ?? extraData.LookInStorage)
                .SlotYielderLabel());

        public static bool ValidLookIn(this ISlotGroupParent parent, Bill_Production bill) => bill.recipe.WorkerCounter.CanPossiblyStore(bill,
            (parent as Zone_Stockpile ?? parent.FakeStockpile()).slotGroup);

        public static string TakeToText(Bill_Production bill, ExtraBillData extraData, out bool incompatible)
        {
            var text = string.Format(bill.GetStoreMode().LabelCap,
                bill.storeGroup is SlotGroup sg ? sg.parent.SlotYielderLabel() :
                extraData.Storage != null ? extraData.Storage.SlotYielderLabel() : "");
            incompatible = bill.storeGroup != null &&
                !bill.recipe.WorkerCounter.CanPossiblyStore(bill, bill.storeGroup) || extraData.Storage != null &&
                !bill.recipe.WorkerCounter.CanPossiblyStore(bill,
                    extraData.Storage.FakeStockpile().slotGroup);
            if (incompatible) text += $" ({"IncompatibleLower".Translate()})";
            return text;
        }

        public static bool ValidTakeTo(this ISlotGroupParent parent, Bill_Production bill)
        {
            return parent switch
            {
                Building_Storage building => bill.recipe.WorkerCounter.CanPossiblyStore(bill, building.FakeStockpile().slotGroup),
                Zone_Stockpile stockpile => bill.recipe.WorkerCounter.CanPossiblyStore(bill, stockpile.slotGroup),
                _ => false
            };
        }
    }
}