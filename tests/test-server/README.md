#
# Copyright (c) 2000, 2024, Oracle and/or its affiliates.
#
# Licensed under the Universal Permissive License v 1.0 as shown at
# http://oss.oracle.com/licenses/upl.
#

The `keystore.jks` was created from `tests\Coherence.Tests\Net\Ssl\server.cer` with the following command:

```shell script
%JAVA_HOME%\bin\keytool -importcert -alias server -file tests\Coherence.Tests\Net\Ssl\server.cer -keypass password -keystore tests\Coherence.Tests\Config\keystore.jks -storepass password
```

Note: when it asks if you trust this cert, say yes.

The `trust.jks` was created from `tests\Coherence.Tests\Net\Ssl\CA.cer` with the following command:

```shell script
%JAVA_HOME%\bin\keytool -importcert -trustcacerts -alias serverCA -file tests\Coherence.Tests\Net\Ssl\CA.cer -keypass password -keystore tests\Coherence.Tests\Config\trust.jks -storepass password
```

Note: when it asks if you trust this cert, say yes.
