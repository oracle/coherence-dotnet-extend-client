using System.Runtime.CompilerServices;
/*
 * Copyright (c) 2000, 2022, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */

// make internals visible to test project
#if RELEASE
[assembly: InternalsVisibleTo("Coherence.Core.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001007db42c04ba971e72a81cf33e038744197afca9c17e842c131463a9eefc815dcba56b49da01d562ca7735b8bb19361b1e1936b98c20a4d65896a474f2b712771fb09310d9df3e0d584a08bfd7f1077098efb74570ed099ba4df36ab22a7efe99a7c82ebdc941a283a2c0cc6a5eb9d8fd7b27c94a18ef24000cb28779d65509ac7")]
#else
[assembly: InternalsVisibleTo("Coherence.Core.Tests")]
#endif