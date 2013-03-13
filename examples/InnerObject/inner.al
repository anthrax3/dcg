@# This template demostrates how to use DCG inner object.
@# It offers facilities to ease template coding.
<html>
<head>
@(Dcg.CallTemplate("header.al", null, null, "Sample"))
</head>
<body>
@(Dcg.CallTemplate("body.al", null, null, 1, 5))
</body>
</html>
