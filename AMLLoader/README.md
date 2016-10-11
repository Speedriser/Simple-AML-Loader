# Simple AML Batch Loader

The Simple AML Batch Loader takes an AML file as input and imports batch of items. It is a simple windows form application.
Any error will not resume and roll-back the entire import, it will divide the failing batch by two and try each batch until the batch are a single item and logged if they still fail.

## Project Details

**Built Using:** Aras 11.0 SP7, Visual Studio 2015
**Platform Tested:** Windows 10

> Though built and tested using Aras 11.0 SP7, this project should function in older releases of Aras 11.0 and Aras 10.0.

## Installation

#### Simply download the latest release or build the project from the sources


## Contributing

1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request

For more information on contributing to this project, another Aras Labs project, or any Aras Community project, shoot us an email at araslabs@aras.com.

## License

Aras Labs projects are published to Github under the MIT license. See the [LICENSE file](./LICENSE.md) for license rights and limitations.