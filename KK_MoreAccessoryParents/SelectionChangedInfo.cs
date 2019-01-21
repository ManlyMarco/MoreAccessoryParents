namespace KK_MoreAccessoryParents
{
    internal struct SelectionChangedInfo
    {
        public readonly ChaAccessoryDefine.AccessoryParentKey AccessoryParentKey;
        public readonly ChaReference.RefObjKey RefObjKey;

        public SelectionChangedInfo(int accessoryParentKey, int refObjKey)
        {
            AccessoryParentKey = (ChaAccessoryDefine.AccessoryParentKey)accessoryParentKey;
            RefObjKey = (ChaReference.RefObjKey)refObjKey;
        }
    }
}