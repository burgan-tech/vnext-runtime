# Version Management and ETag Usage

All business record instances in the Amorphie platform can be managed in versions. The [semantic versioning](https://semver.org/) approach is adopted in versioning. The version is in the standard `minor.major.patch.revision` format.

Version update of the record is determined in workflow definitions. With the property named `VersionStrategy`:
- Version change can be determined at each transition.
- Version change can be determined for each state entry and exit.

<DataSchema id="5391667" />

In addition to versioning, record changes are also managed with the [ETag](https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/ETag) approach. The ETag value is always generated as `ULID`.

The ETag value can also be used for client-side caching.
