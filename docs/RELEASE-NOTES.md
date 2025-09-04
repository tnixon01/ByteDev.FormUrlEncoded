# Release Notes

## 1.3.0 - 4 September 2025

Breaking changes:
- (None) 

New features:
- Add support for multiple deserialization aliases on properties. This is a work-around for some legacy systems that use slightly different naming conventions to submit similar data.
- Add key-to-property map caching for improved performance (theoretical, the performance improvement is not tested).

Bug fixes / internal changes:
- Significant changes to FormUrlEncodedPair that render the prior iteration obsolete. However, since that is not used externally, the changes are not breaking.
- Changes to serialization logic to incorporate caching.

## 1.1.0 - ???

Breaking changes:
- (None)

New features:
- Add handling of `List` property types to strings in `FormUrlEncodedSerializer.Serialize` method.
- Add handling of string collections to `List` property types in `FormUrlEncodedSerializer.Deserialize` method.

Bug fixes / internal changes:
- (None)

## 1.0.0 - 17 June 2021

Initial version.
